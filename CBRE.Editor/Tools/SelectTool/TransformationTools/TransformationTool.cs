﻿using CBRE.DataStructures.Geometric;
using CBRE.Editor.Documents;
using CBRE.Editor.Tools.Widgets;
using CBRE.Editor.Rendering;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace CBRE.Editor.Tools.SelectTool.TransformationTools
{
    public abstract class TransformationTool
    {
        public abstract Matrix GetTransformationMatrix(Viewport2D viewport, ViewportEvent mouseEventArgs, BaseBoxTool.BoxState state, Document doc, IEnumerable<Widget> activeWidgets);
        public abstract bool RenderCircleHandles { get; }
        public abstract bool FilterHandle(BaseBoxTool.ResizeHandle handle);
        public abstract string GetTransformName();
        public abstract MouseCursor CursorForHandle(BaseBoxTool.ResizeHandle handle);
        public abstract IEnumerable<Widget> GetWidgets(Document document);

        /// <summary>
        /// Get a list of handles and their standard offset positions
        /// </summary>
        /// <param name="start">The start coordinate of the box</param>
        /// <param name="end">The end coordinate of the box</param>
        /// <param name="zoom">The zoom value of the viewport</param>
        /// <param name="offset">The offset from the box bounds to place the handles</param>
        /// <returns>A list of handles for the box in tuple form: (Handle, X, Y)</returns>
        public IEnumerable<Tuple<BaseBoxTool.ResizeHandle, decimal, decimal>> GetHandles(Vector3 start, Vector3 end, decimal zoom, decimal offset = 7)
        {
            var half = (end - start) / 2;
            var dist = offset / zoom;

            yield return Tuple.Create(BaseBoxTool.ResizeHandle.TopLeft, start.X - dist, end.Y + dist);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.TopRight, end.X + dist, end.Y + dist);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.BottomLeft, start.X - dist, start.Y - dist);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.BottomRight, end.X + dist, start.Y - dist);

            yield return Tuple.Create(BaseBoxTool.ResizeHandle.Top, start.X + half.X, end.Y + dist);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.Left, start.X - dist, start.Y + half.Y);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.Right, end.X + dist, start.Y + half.Y);
            yield return Tuple.Create(BaseBoxTool.ResizeHandle.Bottom, start.X + half.X, start.Y - dist);
        }

        protected static Vector3 SnapIfNeeded(Vector3 c, Document doc)
        {
            return doc.Snap(c);
        }
    }
}
