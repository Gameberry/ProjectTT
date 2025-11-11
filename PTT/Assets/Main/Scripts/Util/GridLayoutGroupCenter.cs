using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Grid Layout Group Center (Optimized)")]
public class GridLayoutGroupCenter : GridLayoutGroup
{
    private int _cachedChildCount = -1;

    public override void SetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    private void SetCellsAlongAxis(int axis)
    {
        int rectChildrenCount = rectChildren.Count;

        // 최적화: 자식 개수 변경이 없으면 재계산 스킵
        if (axis == 1 && _cachedChildCount == rectChildrenCount)
            return;

        if (axis == 0)
        {
            for (int i = 0; i < rectChildrenCount; i++)
            {
                RectTransform rect = rectChildren[i];
                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = cellSize;
            }
            return;
        }

        _cachedChildCount = rectChildrenCount;

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        int cellCountX = 1;
        int cellCountY = 1;

        switch (constraint)
        {
            case Constraint.FixedColumnCount:
                cellCountX = constraintCount;
                cellCountY = Mathf.CeilToInt(rectChildrenCount / (float)cellCountX);
                break;
            case Constraint.FixedRowCount:
                cellCountY = constraintCount;
                cellCountX = Mathf.CeilToInt(rectChildrenCount / (float)cellCountY);
                break;
            default:
                cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
                break;
        }

        int cornerX = (int)startCorner % 2;
        int cornerY = (int)startCorner / 2;

        int cellsPerMainAxis = (startAxis == Axis.Horizontal) ? cellCountX : cellCountY;
        int actualCellCountX = (startAxis == Axis.Horizontal)
            ? Mathf.Clamp(cellCountX, 1, rectChildrenCount)
            : Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
        int actualCellCountY = (startAxis == Axis.Horizontal)
            ? Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis))
            : Mathf.Clamp(cellCountY, 1, rectChildrenCount);

        int lastRowItemCount = rectChildrenCount % cellsPerMainAxis;
        int cellsX = (startAxis == Axis.Horizontal) ? (lastRowItemCount == 0 ? cellsPerMainAxis : lastRowItemCount) : actualCellCountX;
        int cellsY = (startAxis == Axis.Vertical) ? (lastRowItemCount == 0 ? cellsPerMainAxis : lastRowItemCount) : actualCellCountY;

        Vector2 requiredSpace = new Vector2(
            actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
            actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
        );

        Vector2 offset = new Vector2(
            GetStartOffset(0, requiredSpace.x),
            GetStartOffset(1, requiredSpace.y)
        );

        Vector2 lastRowOffset = new Vector2(
            GetStartOffset(0, cellsX * cellSize.x + (cellsX - 1) * spacing.x),
            GetStartOffset(1, cellsY * cellSize.y + (cellsY - 1) * spacing.y)
        );

        for (int i = 0; i < rectChildrenCount; i++)
        {
            int posX = (startAxis == Axis.Horizontal) ? i % cellsPerMainAxis : i / cellsPerMainAxis;
            int posY = (startAxis == Axis.Horizontal) ? i / cellsPerMainAxis : i % cellsPerMainAxis;

            if (cornerX == 1)
                posX = actualCellCountX - 1 - posX;
            if (cornerY == 1)
                posY = actualCellCountY - 1 - posY;

            bool isInLastRow = (i >= rectChildrenCount - (lastRowItemCount == 0 ? cellsPerMainAxis : lastRowItemCount));
            Vector2 useOffset = isInLastRow ? lastRowOffset : offset;

            SetChildAlongAxis(rectChildren[i], 0, useOffset.x + (cellSize.x + spacing.x) * posX, cellSize.x);
            SetChildAlongAxis(rectChildren[i], 1, useOffset.y + (cellSize.y + spacing.y) * posY, cellSize.y);
        }
    }
}