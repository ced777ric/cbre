﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sledge.DataStructures.GameData;
using Sledge.DataStructures.Geometric;
using Sledge.DataStructures.MapObjects;
using Sledge.Editor.Documents;
using Sledge.FileSystem;
using Sledge.Providers.Model;

namespace Sledge.Editor.Extensions
{
    public static class ModelExtensions
    {
        private const string ModelMetaKey = "Model";
        private const string ModelNameMetaKey = "ModelName";
        private const string ModelBoundingBoxMetaKey = "ModelBoundingBox";

        public static bool UpdateModels(this Map map, Document document)
        {
            if (Sledge.Settings.View.DisableModelRendering) return false;
            return UpdateModels(document, map.WorldSpawn);
        }

        public static bool UpdateModels(this Map map, Document document, IEnumerable<MapObject> objects)
        {
            if (Sledge.Settings.View.DisableModelRendering) return false;

            var updated = false;
            foreach (var mo in objects) updated |= UpdateModels(document, mo);
            return updated;
        }

        private static bool UpdateModels(Document document, MapObject mo)
        {
            var updatedChildren = false;
            foreach (var child in mo.Children) updatedChildren |= UpdateModels(document, child);

            var e = mo as Entity;
            if (e == null || !ShouldHaveModel(e)) return updatedChildren;

            var model = GetModelName(e);
            var existingModel = e.MetaData.Get<string>(ModelNameMetaKey);
            if (String.Equals(model, existingModel, StringComparison.InvariantCultureIgnoreCase)) return updatedChildren; // Already set; No need to continue

            var file = document.Environment.Root.TraversePath(model);
            if (file == null || !ModelProvider.CanLoad(file))
            {
                // Model not valid, get rid of it
                UnsetModel(e);
                return true;
            }

            try
            {
                SetModel(e, ModelProvider.CreateModelReference(file));
                return true;
            }
            catch
            {
                // Couldn't load
                return updatedChildren;
            }
        }

        private static bool ShouldHaveModel(Entity entity)
        {
            return GetModelName(entity) != null;
        }

        private static string GetModelName(Entity entity)
        {
            if (entity.GameData == null) return null;
            var studio = entity.GameData.Behaviours.FirstOrDefault(x => String.Equals(x.Name, "studio", StringComparison.InvariantCultureIgnoreCase))
                         ?? entity.GameData.Behaviours.FirstOrDefault(x => String.Equals(x.Name, "sprite", StringComparison.InvariantCultureIgnoreCase));
            if (studio == null) return null;

            // First see if the studio behaviour forces a model...
            if (String.Equals(studio.Name, "studio", StringComparison.InvariantCultureIgnoreCase)
                && studio.Values.Count == 1 && !String.IsNullOrWhiteSpace(studio.Values[0]))
            {
                return studio.Values[0].Trim();
            }

            // Find the first property that is a studio type, or has a name of "model"...
            var prop = entity.GameData.Properties.FirstOrDefault(x => x.VariableType == VariableType.Studio);
            if (prop == null) prop = entity.GameData.Properties.FirstOrDefault(x => String.Equals(x.Name, "model", StringComparison.InvariantCultureIgnoreCase));
            if (prop != null)
            {
                var val = entity.EntityData.GetPropertyValue(prop.Name);
                if (!String.IsNullOrWhiteSpace(val)) return val;
            }
            return null;
        }

        private static void SetModel(Entity entity, ModelReference model)
        {
            entity.MetaData.Set(ModelMetaKey, model);
            entity.MetaData.Set(ModelNameMetaKey, GetModelName(entity));
            entity.MetaData.Set(ModelBoundingBoxMetaKey, (Box) null); //todo.
        }

        private static void UnsetModel(Entity entity)
        {
            entity.MetaData.Unset(ModelMetaKey);
            entity.MetaData.Unset(ModelNameMetaKey);
            entity.MetaData.Unset(ModelBoundingBoxMetaKey);
        }

        public static ModelReference GetModel(this Entity entity)
        {
            return entity.MetaData.Get<ModelReference>(ModelMetaKey);
        }

        public static bool HasModel(this Entity entity)
        {
            return entity.MetaData.Has<ModelReference>(ModelMetaKey);
        }

        public static int HideDistance(this Entity entity)
        {
            return HasModel(entity) ? Sledge.Settings.View.ModelRenderDistance : int.MaxValue;
        }
    }
}
