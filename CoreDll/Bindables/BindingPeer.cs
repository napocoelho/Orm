using System;

namespace CoreDll.Bindables
{

    public class BindingPeer<TLeftProperty, TRightProperty> : IBindingPeer
    {
        //WeakReference<IBindingPeer> teste;

        private object _LOCK_;
        private bool _LOOPING_PREVENT_ = false;

        /// <summary>
        /// Activates or deactivates the Bind.
        /// </summary>
        public bool Activated { get; set; }

        public UpdatingWay Way { get; set; }

        public LazyValueBag LastChangedValue { get; protected set; }

        public Func<TRightProperty, TLeftProperty> ConvertToLeft { get; protected set; }
        public Func<TLeftProperty, TRightProperty> ConvertToRight { get; protected set; }

        public AbstractBindingTrigger LeftTrigger { get; protected set; }
        public AbstractBindingTrigger RightTrigger { get; protected set; }

        public BindingPeer(AbstractBindingTrigger leftTrigger, AbstractBindingTrigger rightTrigger,
                           Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
                           UpdatingWay updateWay = UpdatingWay.Both, Way startsWith = Bindables.Way.ToRight)
        {
            Activated = true;


            LeftTrigger = leftTrigger;
            RightTrigger = rightTrigger;

            ConvertToLeft = conversionToLeft;
            ConvertToRight = conversionToRight;

            if (LeftTrigger != null)
            {
                LeftTrigger.ValueChanged += LeftValueChanged;
            }

            if (RightTrigger != null)
            {
                RightTrigger.ValueChanged += RightValueChanged;
            }

            Way = updateWay;

            _LOCK_ = new object(); //--> objeto para fazer os locks (garantir consistência);
            LastChangedValue = new LazyValueBag(_LOCK_);

            // começar com o primeiro valor:
            if (startsWith == Bindables.Way.ToRight)
            {
                LeftValueChanged(null, null);
            }
            else if (startsWith == Bindables.Way.ToLeft)
            {
                RightValueChanged(null, null);
            }
        }

        private void LeftValueChanged(object sender, EventArgs e)
        {
            if (!Activated)
                return;

            if (_LOOPING_PREVENT_)
                return;

            if (ConvertToRight == null)
                return;

            object value = LeftTrigger.PropertyInfo.GetValue(LeftTrigger.Source);

            LazyValue lazyValue = new LazyValue();
            lazyValue.Value = ConvertToRight((TLeftProperty)value);
            lazyValue.Direction = Direction.ToRight;
            LastChangedValue.SetValue(lazyValue);

            if (Way == UpdatingWay.Both || Way == UpdatingWay.LeftToRight)
            {
                UpdatePeers();
            }
        }

        private void RightValueChanged(object sender, EventArgs e)
        {
            if (!Activated)
                return;

            if (_LOOPING_PREVENT_)
                return;

            if (ConvertToLeft == null)
                return;

            object value = RightTrigger.PropertyInfo.GetValue(RightTrigger.Source);

            LazyValue lazyValue = new LazyValue();
            lazyValue.Value = ConvertToLeft((TRightProperty)value);
            lazyValue.Direction = Direction.ToLeft;
            LastChangedValue.SetValue(lazyValue);

            if (Way == UpdatingWay.Both || Way == UpdatingWay.RightToLeft)
            {
                UpdatePeers();
            }
        }

        public void UpdatePeers()
        {
            if (!Activated)
                return;

            lock (_LOCK_)
            {
                LazyValue value;

                if (LastChangedValue.TryGetValue(out value))
                {
                    _LOOPING_PREVENT_ = true;

                    if (value.Direction == Direction.ToLeft)
                    {
                        LeftTrigger.PropertyInfo.SetValue(LeftTrigger.Source, value.Value);
                    }

                    if (value.Direction == Direction.ToRight)
                    {
                        RightTrigger.PropertyInfo.SetValue(RightTrigger.Source, value.Value);
                    }

                    _LOOPING_PREVENT_ = false;
                }
            }
        }
    }
}
