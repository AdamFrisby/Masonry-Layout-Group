using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SineWave.Sinespace.Assets.Runtime_Project.Code.NewUI.Extend
{
    public class MasonryLayoutGroup : LayoutGroup
    {
        public int TargetCellsWide = 5;
        public float TargetCellAspect = 16f / 9f;

        public float Spacing = 8f;

        public override void CalculateLayoutInputVertical()
        {
            SetLayout();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            SetLayout();
        }

        public override void SetLayoutHorizontal()
        {
            SetLayout();
        }

        public override void SetLayoutVertical()
        {
            SetLayout();
        }
        
        private readonly Dictionary<int,int> _cellWidths = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _cellHeights = new Dictionary<int, int>();
        private int[,] _cellInUse;

        private void SetLayout()
        {
            if (TargetCellsWide < 1)
                TargetCellsWide = 1;
            
            var workingWidth = rectTransform.rect.width - padding.left - padding.right;

            var targetCellWidth = (workingWidth - (Spacing * (TargetCellsWide - 1))) / TargetCellsWide;
            
            _cellWidths.Clear();
            _cellHeights.Clear();

            const int maxCellHeightMultiple = 10;

            var cellArrayHeight = Mathf.Max(maxCellHeightMultiple * 2, rectChildren.Count * TargetCellsWide);

            if (_cellInUse == null || _cellInUse.GetLength(0) != TargetCellsWide + 1 ||
                _cellInUse.GetLength(1) != cellArrayHeight)
                _cellInUse = new int[TargetCellsWide + 1, cellArrayHeight];

            for (int x1 = 0; x1 < _cellInUse.GetLength(0); x1++)
            {
                for (int y1 = 0; y1 < _cellInUse.GetLength(1); y1++)
                {
                    _cellInUse[x1, y1] = -1;
                }
            }

            for (int i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];

                int targetCellsWide = Mathf.Min(Mathf.Max(1,
                    Mathf.RoundToInt(LayoutUtility.GetPreferredWidth(child) / targetCellWidth)), TargetCellsWide);

                
                int targetCellsHigh = Mathf.Min(maxCellHeightMultiple, Mathf.Max(1,
                    Mathf.RoundToInt(LayoutUtility.GetPreferredHeight(child) / (targetCellWidth * TargetCellAspect))));

                _cellWidths[i] = targetCellsWide;
                _cellHeights[i] = targetCellsHigh;

                // Find a place for this cell.
                bool found = false;
                for (int y1 = 0; y1 < rectChildren.Count; y1++)
                {
                    for (int x1 = 0; x1 < TargetCellsWide; x1++)
                    {
                        if (targetCellsWide > 1 || targetCellsHigh > 1)
                        {
                            bool skip = false;

                            for (int x2 = 0; x2 < targetCellsWide; x2++)
                            {
                                for (int y2 = 0; y2 < targetCellsHigh; y2++)
                                {
                                    if ((x1 + x2) >= TargetCellsWide || _cellInUse[x1 + x2, y1 + y2] != -1)
                                    {
                                        skip = true;
                                        break;
                                    }
                                }

                                if (skip)
                                    break;
                            }

                            if (!skip)
                            {
                                for (int x2 = 0; x2 < targetCellsWide; x2++)
                                {
                                    for (int y2 = 0; y2 < targetCellsHigh; y2++)
                                    {
                                        _cellInUse[x1 + x2, y1 + y2] = i;
                                    }
                                }

                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            if (_cellInUse[x1, y1] == -1)
                            {
                                _cellInUse[x1, y1] = i;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                        break;
                }

                if (!found)
                    Debug.LogWarning("Couldn't find a empty spot for " + rectChildren[i].name + " in masonry layout");
            }

            int modified = 0;

            for (int x = 0; x < TargetCellsWide; x++)
            {
                bool rowHasContents = false;

                for(int y = 0; y < rectChildren.Count; y++)
                {
                    var index = _cellInUse[x, y];

                    if (index >= 0)
                    {
                        if (_cellWidths.ContainsKey(index) && _cellWidths[index] > 0)
                        {
                            float width = (_cellWidths[index] * targetCellWidth) +
                                          (_cellWidths[index] > 1 ? (Spacing * (_cellWidths[index] - 1)) : 0);

                            float oneCellHeight = targetCellWidth * TargetCellAspect;

                            float height = (_cellHeights[index] * oneCellHeight) +
                                          (_cellHeights[index] > 1 ? (Spacing * (_cellHeights[index] - 1)) : 0);

                            float left = x * targetCellWidth + (x > 0 ? (x - 0) * Spacing : 0) + padding.left;
                            float top = y * oneCellHeight + (y > 0 ? (y - 0) * Spacing : 0) + padding.top;

                            SetChildAlongAxis(rectChildren[index], 0, left, width);
                            SetChildAlongAxis(rectChildren[index], 1, top, height);

                            _cellWidths[index] = -1; // Been found, skip subsequent rows.

                            rowHasContents = true;
                            modified++;
                        }
                    }
                }

                //if (!rowHasContents)
                //    break;
            }

            if (modified != rectChildren.Count)
            {
                Debug.LogWarning("Warning: Masonry layout did not modify all children? Expected " + rectChildren.Count +
                                 " modified " + modified);
            }

        }
    }
}
