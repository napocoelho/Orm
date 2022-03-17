namespace CoreDll.Bindables
{
    public interface IBindingPeer
    {
        bool Activated { get; set; }

        UpdatingWay Way { get; set; }

        LazyValueBag LastChangedValue { get; }

        void UpdatePeers();
    }
}
