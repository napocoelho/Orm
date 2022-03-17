using CoreDll.Bindables;
using System.Collections.Generic;
using System.Linq;

namespace CoreDll.Utils
{
    public class CircularList<T> : BindableBase
    {
        private List<T> InternalItems { get; set; }
        public IReadOnlyList<T> Items { get; set; }

        private int? SelectedIndex
        {
            get { return base.Get<int?>(); }
            set
            {
                int? backup = SelectedIndex;

                if (base.Set(value))
                {
                    SelectedItem = SelectedIndex is null ? default : InternalItems[SelectedIndex.Value];
                    HasItems = InternalItems.Any();
                    IsSelected = !(SelectedIndex is null);
                    IsFirstItemSelected = SelectedIndex is null || SelectedIndex == 0;
                    IsLastItemSelected = SelectedIndex is null || SelectedIndex == (InternalItems.Count - 1);
                }
            }
        }



        public T SelectedItem { get { return base.Get<T>(); } private set { base.Set(value); } }
        public bool HasItems { get { return base.Get<bool>(); } private set { base.Set(value); } }
        public bool IsSelected { get { return base.Get<bool>(); } private set { base.Set(value); } }
        public bool IsFirstItemSelected { get { return base.Get<bool>(); } private set { base.Set(value); } }
        public bool IsLastItemSelected { get { return base.Get<bool>(); } private set { base.Set(value); } }




        public CircularList()
        {
            SelectedIndex = null;
            InternalItems = new List<T>();
            Items = InternalItems;
        }

        public CircularList(IEnumerable<T> list)
            : this()
        {
            InternalItems.AddRange(list);
        }



        public void AddFirst(T item)
        {
            InternalItems.Insert(0, item);
            base.OnInternalPropertyChanged(nameof(Items));

            if (IsSelected)
                SelectedIndex++;
        }

        public void AddLast(T item)
        {
            InternalItems.Add(item);
            base.OnInternalPropertyChanged(nameof(Items));
        }

        public void RemoveFirst()
        {
            if (HasItems)
            {
                RemoveIndex(0);
            }

            if (!HasItems)
            {
                SelectedIndex = null;
            }
        }

        public void RemoveLast()
        {
            if (HasItems)
            {
                RemoveIndex(InternalItems.Count - 1);
            }

            if (!HasItems)
            {
                SelectedIndex = null;
            }
        }

        public void RemoveIndex(int index)
        {
            if (InternalItems.Any() && index >= 0 && index < InternalItems.Count)
            {
                InternalItems.RemoveAt(index);
                base.OnInternalPropertyChanged(nameof(Items));

                if (HasItems && IsSelected && SelectedIndex >= index)
                {
                    SelectedIndex--;
                }

                if (!HasItems)
                {
                    SelectedIndex = null;
                }
            }
        }

        public void Deselect()
        {
            SelectedIndex = null;
        }

        public void SelectFirstItem()
        {
            if (HasItems)
                SelectedIndex = 0;
        }

        public void SelectLastItem()
        {
            if (HasItems)
                SelectedIndex = InternalItems.Count - 1;
        }

        public void SelectIndex(int? index)
        {
            if (HasItems)
            {
                if (index is null)
                {
                    SelectedIndex = null;
                }
                else if (index >= 0 && index < InternalItems.Count)
                {
                    SelectedIndex = index;
                }
            }
        }

        public void SelectNext()
        {
            if (InternalItems.Any())
            {
                if (SelectedIndex is null)
                {
                    SelectedIndex = 0;
                }
                else if (SelectedIndex < InternalItems.Count)
                {
                    SelectedIndex++;
                }
                else
                {
                    SelectedIndex = 0;
                }
            }
        }

        public void SelectPrevious()
        {
            if (InternalItems.Any())
            {
                if (SelectedIndex is null)
                {
                    SelectedIndex = InternalItems.Count - 1;
                }
                else if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                else
                {
                    SelectedIndex = InternalItems.Count - 1;
                }
            }
        }

    }
}