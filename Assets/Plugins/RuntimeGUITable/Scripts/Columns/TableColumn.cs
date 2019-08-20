using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

namespace UnityUITable
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(VerticalLayoutGroup))]
	public class TableColumn : MonoBehaviour
	{

		Table _table;
		public Table table
		{
			get
			{
				if (_table == null)
					_table = GetComponentInParent<Table>();
				return _table;
			}
		}

		public TableColumnInfo info { get { return table.GetColumnInfoAt(columnIndex); } }
		[SerializeField][HideInInspector] protected LayoutElement layoutElement;

		[SerializeField][HideInInspector] protected VerticalLayoutGroup columnLayout;
		public int columnIndex;

		[SerializeField][HideInInspector] protected List<CellContainer> cellContainers = new List<CellContainer>();

		[SerializeField][HideInInspector] protected CellContainer addButtonCellContainer;

		public void Initialize(int columnIndex)
		{
			transform.DestroyChildrenImmediate();
			cellContainers.Clear();
			this.columnIndex = columnIndex;
			columnLayout = gameObject.GetOrAddComponent<VerticalLayoutGroup>();
			columnLayout.childControlWidth = true;
			columnLayout.childControlHeight = true;
			columnLayout.childForceExpandWidth = true;
			columnLayout.childForceExpandHeight = true;
			columnLayout.spacing = -1f;
			ContentSizeFitter sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			layoutElement = gameObject.AddComponent<LayoutElement>();

			CreateTitleCell();

			CreateCells();

			Update();
		}

		void CreateAddButton()
		{
			addButtonCellContainer = GameObjectUtils.InstantiatePrefab(table.addButtonCellContainerPrefab, transform);
			addButtonCellContainer.Initialize();
			ButtonCell cell = (ButtonCell)addButtonCellContainer.CreateCellContent(table.addCellPrefab);
			cell.Initialize();
		}

		protected virtual void CreateTitleCell()
		{
			HeaderCellContainer title = GameObjectUtils.InstantiatePrefab(table.columnTitlePrefab, transform);
			title.Initialize();
		}

		protected virtual void CreateCells()
		{
			int elmtCount = table.ElementCount;
			for (int rowIndex = 0; rowIndex < elmtCount; rowIndex++)
			{
				CreateCell(rowIndex);
			}
		}

		protected virtual CellContainer CreateCell(int rowIndex)
		{
			TableCellContainer cellContainer = GameObjectUtils.InstantiatePrefab(table.cellContainerPrefab, transform);
			cellContainer.Initialize(rowIndex);
			cellContainers.Add(cellContainer);
			TableCell contentInstance = cellContainer.CreateCellContent(info.CellPrefab);
			contentInstance.Initialize();
			return cellContainer;
		}

		protected CellContainer AddEmptyCell(string name)
		{
			GameObject emptyTitleGO = transform.CreateChildGameObject(name);
			CellContainer emptyCell = emptyTitleGO.AddComponent<CellContainer>();
			emptyCell.Initialize();
			return emptyCell;
		}

		protected void AddEmptyAddButtonRowCell()
		{
			addButtonCellContainer = AddEmptyCell("Empty Add Row Cell");
		}

		protected void Update()
		{
			if (table == null || info == null)
				return;
			UpdateAddButtonRow();
			layoutElement.preferredWidth = info.AbsoluteWidth;
			columnLayout.spacing = table.spacing;
		}

		void UpdateAddButtonRow()
		{
			if (table.rowAddButton && addButtonCellContainer == null)
			{
				if (columnIndex == 0)
					CreateAddButton();
				else
					AddEmptyAddButtonRowCell();
			}
			else if (!table.rowAddButton && addButtonCellContainer != null)
				DestroyImmediate(addButtonCellContainer.gameObject);
		}

		void UpdateRows(int expectedNbRows, int actualNbRows, int bottomRows)
		{
			if (actualNbRows < expectedNbRows)
			{
				int nbToAdd = expectedNbRows - actualNbRows;
				for (int i = 0; i < nbToAdd; i++)
				{
					int rowIndex = actualNbRows + i - 1 - bottomRows;CreateCell(rowIndex);
				}
			}
			else if (actualNbRows > expectedNbRows)
			{
				int nbToRemove = actualNbRows - expectedNbRows;
				int removalIndex = expectedNbRows - 1 - bottomRows;
				for (int i = 0; i < nbToRemove; i++)
				{
					DestroyImmediate(cellContainers[removalIndex].gameObject);
					cellContainers.RemoveAt(removalIndex);
				}
			}
		}
		 
		public void TitleClicked()
		{
			table.ColumnTitleClicked(info);
		}

		public int IndexOf(CellContainer cellContainer)
		{
			return cellContainers.IndexOf(cellContainer);
		}

		public CellContainer GetCellAt(int index)
		{
			try
			{
				return cellContainers[index];
			}
			catch(System.IndexOutOfRangeException)
			{
				return null;
			}
		}

		public void UpdateContent()
		{
			UpdateAddButtonRow();
			int bottomRows = (table.rowAddButton ? 1 : 0);
			int expectedNbRows = table.ElementCount + 1 + bottomRows;
			int actualNbRows = transform.childCount;
			if (actualNbRows != expectedNbRows)
				UpdateRows(expectedNbRows, actualNbRows, bottomRows);
			UpdateCellsActiveState();
			foreach (CellContainer cell in cellContainers)
				cell.UpdateContent();
		}

		public void UpdateStyle()
		{
			foreach (CellContainer cell in cellContainers)
				cell.UpdateStyle();
			if (addButtonCellContainer != null)
				addButtonCellContainer.UpdateStyle();
		}

		void UpdateCellsActiveState()
		{
			for (int i = 0; i < cellContainers.Count; i++)
			{
				CellContainer cell = cellContainers[i];
				cell.gameObject.SetActive(Application.isPlaying || !table.limitRowsInEditMode || i < table.nbRowsInEditMode || cell == addButtonCellContainer);
			}
		}

	}

}
