using System;
using System.Collections;

namespace Raona.Framework.Collections
{
	/// <summary>
	/// Creates a Collection of items where each item has one and only one Parent and zero or more childs.
	/// The structure is not logically controled: the parent of a item can be itself, or one of its childs!
	/// Note that a single ROOT is not a constraint, so no property called Root (or similar) exists.
	/// </summary>
	public class UnorderedTree
	{

		/// <summary>
		/// Delegate that is used to "sort" the tree into an ArrayList.
		/// </summary>
		public delegate int OrderItemDelegate (TreeItem item1, TreeItem item2);

		/// <summary>
		/// Delegate that is used to find an specified item.
		/// Must return true if item is the item searched
		/// </summary>
		public delegate bool FindItemDelegate (TreeItem item, object data);

		/// <summary>
		///  Delegate used to "evaluate" an item, and returning anything about it.
		/// </summary>
		public delegate object EvalItem (TreeItem item);

		protected ArrayList _items;		// ArrayList of TreeItem, containing ALL the tree
		EvalItem _evalHandler;			// Eval method handler

		/// <summary>
		/// Denotes an Item of the Tree. An item is some data (object) any number of parents and an array of childs (items)
		/// </summary>
		public class TreeItem
		{
			ArrayList _parents;			// Parents of TreeItem
			ArrayList _childs;			// ArrayList of TreeItem
			object _data;				// Data used
			internal UnorderedTree _tree;	// The tree of that item
			

			/// <summary>
			/// Constructs a new Treeitem with specified data and parent
			/// </summary>
			/// <param name="data">Data of the item</param>
			/// <param name="parent">Parent of the item (another item). CAN be null (if null the item is Orphan)</param>
			internal TreeItem (object data, TreeItem parent)
			{
				_childs = new ArrayList();
				_data = data;
				_parents = new ArrayList();
				_tree = null;
				// If we have a parent, the parent must have us as a child
				if (parent!=null) 
				{
					_parents.Add(parent);
					parent._childs.Add (this);
					_tree = parent._tree;
				}
			}

			/// <summary>
			/// Add a new parent to the current item.
			/// </summary>
			/// <param name="parent">New parent to add</param>
			protected void AddParent (TreeItem parent)
			{
				if (parent._tree != _tree) return;			// Cannot add a parent of another tree.
				if (!_parents.Contains(parent)) {
					// Add the parent at our list of parents
					_parents.Add (parent);
					// Add US to the childs list of our parent.
					parent._childs.Add (this);
				}
			}

			/// <summary>
			/// Gets and sets the current data of the item.
			/// </summary>
			public object Data
			{
				get { return _data; }
				set { _data = value; }
			}

			/// <summary>
			///  Adds a new item as a child of the current item
			/// </summary>
			/// <param name="item">Item to be the new child</param>
			public void AddChild (TreeItem item)
			{
				item.AddParent (this);
			}

			/// <summary>
			/// Gets a newly created ArrayList with all descendants of the current item.
			/// Descendants are the childs, the childs of the childs and so on...
			/// </summary>
			public ArrayList GetDescendants()
			{
				ArrayList arr = new ArrayList();
				GetDescendants(ref arr);
				return arr;
			}

			/// <summary>
			/// Get all descendants of the current item, and recursivily all the descendants of the rest
			/// of items
			/// </summary>
			/// <param name="toAdd">Arraylist where to add the descendants.</param>
			protected void GetDescendants (ref ArrayList toAdd)
			{
				foreach (object o in _childs)
				{
					toAdd.Add ((TreeItem)o);
					((TreeItem)o).GetDescendants(ref toAdd);
				}
			}

			/// <summary>
			/// That method functions as LISP's mapcar function: evaluates a list (ArrayList in that case) and apply
			/// a method over all items of that list. The result is another list containing the same number of elements
			/// of the first list.
			/// That method returns the ArrayList that is obtained to evaluate all descendants items of the current item.
			/// Note that calling MapcarDescendants is the same to call Mapcar (GetDescendants()) but is more efficiently.
			/// </summary>
			/// <returns>The arraylist of mapcar'ed descendants.</returns>
			public ArrayList MapcarDescendants()
			{
				ArrayList arr = new ArrayList();
				MapcarDescendants (ref arr);
				return arr;
			}

			/// <summary>
			/// Get all descendants of the current item, and for each descendant evaluates it, and stores
			/// the result of the evaluation in the ArrayList
			/// </summary>
			/// <param name="toAdd">ArrayList where to add the evaluation results</param>
			protected void MapcarDescendants(ref ArrayList toAdd)
			{
				foreach (object o in _childs)
				{
					toAdd.Add (_tree.Eval ((TreeItem)o));
					((TreeItem)o).MapcarDescendants (ref toAdd);
				}
			}

			/// <summary>
			///  Gets an newly created ArrayList with all childs of the current item
			/// </summary>
			public ArrayList Childs
			{
				get
				{
					ArrayList ret = new ArrayList();
					foreach (object o in _childs)
					{
						ret.Add ((TreeItem)o);		
					}
					return ret;
				}
			}

			/// <summary>
			/// Gets the number of childs of the current item.
			/// </summary>
			public int ChildsCount
			{
				get { return _childs.Count; }
			}


			/// <summary>
			/// Gets if the item is Orphan (has no parents).
			/// </summary>
			public bool IsOrphan { get { return _parents.Count == 0; } }

			/// <summary>
			/// Gets an ArrayList with all parents of the current item
			/// </summary>
			public ArrayList Parents
			{ 
				get
				{
					ArrayList ret = new ArrayList();
					foreach (object o in _parents)
					{
						ret.Add ((TreeItem)o);		
					}
					return ret;
				}
			}
		}

		/// <summary>
		/// Builds a new empty tree.
		/// </summary>
		public UnorderedTree()
		{
			_items = new ArrayList();
		}

		/// <summary>
		/// Adds an Orphan item to the tree.
		/// </summary>
		/// <param name="item">TreeItem to add</param>
		public void Add (TreeItem item)
		{
			if (!_items.Contains(item))
			{
				_items.Add(item);
				item._tree = this;
			}
		}

		/// <summary>
		/// Adds the object o to the tree. A new orphan TreeItem is builded and added to the tree.
		/// </summary>
		/// <param name="o">Data to add to a new item</param>
		/// <returns>The TreeItem added to the tree</returns>
		public TreeItem Add (object o)
		{
			TreeItem item = new TreeItem (o, null);
			Add (item);
			item._tree = this;
			return item;
		}

		/// <summary>
		/// Gets all items with NO parent
		/// </summary>
		/// <returns>An ArrayList of TreeItems with no parent assigned.</returns>
		public ArrayList FindOrphans()
		{
			ArrayList ret = new ArrayList();
			foreach (object o in _items)
			{
				TreeItem item = (TreeItem)o;
				if (item.IsOrphan) ret.Add (item);
			}
			return ret;
		}

		/// <summary>
		/// Returns a new ArrayList with all items to the tree
		/// </summary>
		/// <returns>An ArrayList with all items of the tree. The order is NOT specified.</returns>
		public virtual ArrayList ToArrayList()
		{
			ArrayList arr = new ArrayList();
			foreach (object o in _items)
			{
				arr.Add ((TreeItem)o);
			}
			return arr;
		}
		/// <summary>
		/// Returns a new ArrayList with all items of the tree, sorted by pOrderMethod delegate.
		/// The delegate must adhere to the ICompare rules:
		///		Return -1 if item1 < item2.
		///		Return 0 if item1 == item2.
		///		Return 1 if item1 > item2.
		/// </summary>
		/// <param name="pOrderMethod">Delegate used to sort the arrayList</param>
		/// <returns>The ArrayList with the contents of the tree</returns>
		public virtual ArrayList ToArrayList (OrderItemDelegate pOrderMethod)
		{
			ArrayList arr = ToArrayList();
			// Bubble-sort the ArrayList using pOrderMethod.
			if (pOrderMethod != null)
			{
				arr.Sort (new UnorderedTreeComparer(pOrderMethod));
			}
			return arr;
		}

		/// <summary>
		/// Returns a new ArrayList with all items of the tree sorted by the specified
		/// comparer. Remember that the comparer will receive UnorderedTree::TreeItem objects
		/// in their Compare method.
		/// </summary>
		/// <param name="comparer">Comparer used to sort the list</param>
		/// <returns>The ArrayList with the contents of the tree</returns>
		public virtual ArrayList ToArrayList (IComparer comparer)
		{
			ArrayList arr = ToArrayList	();
			if (comparer != null) 
			{
				arr.Sort (comparer);
			}
			return arr;
		}

		/// <summary>
		/// Finds an specified TreeItem that asserts a condition.
		/// </summary>
		/// <param name="pFindMethod">Delegate used to specify if a TreeItem reaches the condition.
		/// Must return true if the TreeItem reaches the condition.</param>
		/// <param name="data">Object passed to delegate method</param>
		/// <returns>The 1st TreeItem that reaches the condition (or NULL if none)</returns>
		public TreeItem FindItem (FindItemDelegate pFindMethod, object data)
		{
			return FindItem (pFindMethod, data, 1);
		}

		/// <summary>
		/// Finds an specified TreeItem that asserts a condition.
		/// </summary>
		/// <param name="pFindMethod">Delegate used to specify if a TreeItem reaches the condition.
		/// Must return true if the TreeItem reaches the condition.</param>
		/// <param name="data">Object passed to delegate method</param>
		/// <param name="nItem">The ordinal of TreeItem that reaches the condition (i.e. if nItem equals 2 the
		/// second TreeItem that reaches the condition will be returned).</param>
		/// <returns>The nItem-nth TreeItem that reaches the condition (or NULL if none)</returns>
		public TreeItem FindItem (FindItemDelegate pFindMethod, object data, int nItem)
		{
			int nfind = 0;
			foreach (object o in _items)
			{
				if (pFindMethod ((TreeItem)o, data) && ++nfind == nItem) {
					return (TreeItem)o;
				}
			}
			return null;
		}

		/// <summary>
		/// Sets the delegate used to evaluate the items of the tree
		/// </summary>
		public EvalItem EvalHandler { set { _evalHandler = value; } }

		/// <summary>
		/// Evaluates an item. Evaluate an item means to call a delegate method over that
		/// item. The delegate used to evaluate all items of a specified tree is specified
		/// by the EvalHandler property.
		/// </summary>
		/// <returns>The result of the evaluation (an object) or null if item was null or were not in the tree</returns>
		public object Eval(TreeItem item) 
		{ 
			if (!_items.Contains(item) || item== null) return null;
			else return _evalHandler != null ? _evalHandler(item) : null; 
		}

		/// <summary>
		/// That method functions as LISP's mapcar function: for each element of ArrayList arr, apply a function
		/// (in that case is the EvalHandler) and stores the result of evaluation in a new ArrayList.
		/// The new ArrayList contains one element for each element of arr
		/// </summary>
		/// <param name="arr">ArrayList to evaluate</param>
		/// <returns>ArrayList with all evaluations</returns>
		public ArrayList Mapcar (ArrayList arr)
		{
			ArrayList ret = new ArrayList (arr.Count);
			foreach (object o in arr)
			{
				TreeItem titem  = o as TreeItem;
				if (titem != null) 
				{
					ret.Add (titem._tree.Eval (titem));
				}
				else ret.Add (null);
			}
			return ret;
		}


		/// <summary>
		/// That class allows to sort an ArrayList through an OrderItemDelegate
		/// </summary>
		internal class UnorderedTreeComparer : IComparer
		{
			private OrderItemDelegate _pMethod;

			#region IComparer Members

			internal UnorderedTreeComparer (OrderItemDelegate pOrderMethod)
			{
				_pMethod = pOrderMethod;
			}

			public int Compare(object x, object y)
			{
				return _pMethod ((UnorderedTree.TreeItem)x,(UnorderedTree.TreeItem)y);
			}

			#endregion
		}
	}
}
