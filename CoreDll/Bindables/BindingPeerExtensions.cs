using System;
using System.Linq.Expressions;

//using System.Windows.Forms;

using CoreDll.LinqExpressions;

namespace CoreDll.Bindables
{


    public static class BindingPeerExtensions
    {
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Control To Control

        ///// <summary>
        ///// Binds a control to another control.
        ///// </summary>
        //public static IBindingPeer BindsToControl<TControl, TRightControl, TLeftProperty, TRightProperty>(this TControl control, Expression<Func<TControl, TLeftProperty>> controlMember, string controlEventName,
        //                                                            TRightControl rightControl, Expression<Func<TRightControl, TRightProperty>> rightControlMember, string rightControlEventName,
        //                                                            Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                                            UpdatingWay updatingWay = UpdatingWay.Both)
        //    where TControl : Control
        //    where TRightControl : Control
        //{
        //    EventHandlerBindingTrigger<TControl> controlTrigger = new EventHandlerBindingTrigger<TControl>(controlEventName, control, ExprenssionHelper.MemberName(controlMember));
        //    EventHandlerBindingTrigger<TRightControl> rightControlTrigger = new EventHandlerBindingTrigger<TRightControl>(rightControlEventName, rightControl, ExprenssionHelper.MemberName(rightControlMember));

        //    BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(controlTrigger, rightControlTrigger, conversionToLeft, conversionToRight, updatingWay);

        //    DataBindingRegistry.Register(control.FindForm(), bind);
        //    return bind;
        //}

        ///// <summary>
        ///// Binds a control to another control.
        ///// </summary>
        //public static IBindingPeer BindsToControl<TControl, TRightControl, TLeftProperty, TRightProperty>(this TControl control, Expression<Func<TControl, TLeftProperty>> controlMember,
        //                                                            TRightControl rightControl, Expression<Func<TRightControl, TRightProperty>> rightControlMember,
        //                                                            Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                                            string bothSideControlEventName,
        //                                                            UpdatingWay updatingWay = UpdatingWay.Both)
        //    where TControl : Control
        //    where TRightControl : Control
        //{

        //    return BindsToControl<TControl, TRightControl, TLeftProperty, TRightProperty>(control, controlMember, bothSideControlEventName,
        //                                                                    rightControl, rightControlMember, bothSideControlEventName,
        //                                                                    conversionToLeft, conversionToRight,
        //                                                                    updatingWay);
        //}


        ///// <summary>
        ///// Binds a control to another control.
        ///// </summary>
        //public static IBindingPeer BindsToControl<TControl, TRightControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                                            TRightControl rightControl, Expression<Func<TRightControl, TProperty>> rightControlMember, string rightControlEventName,
        //                                                            UpdatingWay updatingWay = UpdatingWay.Both)
        //    where TControl : Control
        //    where TRightControl : Control
        //{
        //    Func<TProperty, TProperty> conversion = (value) => { return value; };
        //    return BindsToControl<TControl, TRightControl, TProperty, TProperty>(control, controlMember, controlEventName,
        //                                                                    rightControl, rightControlMember, rightControlEventName,
        //                                                                    conversion, conversion,
        //                                                                    updatingWay);

        //}

        #endregion Control To Control

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Control To Data Source

        ///// <summary>
        ///// Binds a control to a data source.
        ///// </summary>
        ///// <typeparam name="TS">Source Type</typeparam>
        ///// <typeparam name="TL">Left Source (control) Type</typeparam>
        ///// <typeparam name="TR">Right Source (data source) Type</typeparam>
        ///// <param name="control">A Windows Form Control</param>
        ///// <param name="controlMember">A property from the control</param>
        ///// <param name="controlEvent">An event from the control</param>
        ///// <param name="dataSource">A data source (a POCO object or something that implements [System.ComponentModel.INotifyPropertyChanged])</param>
        ///// <param name="dataSourceMember">A data source property</param>
        ///// <param name="conversionToLeft">An action that will convert the value from the right side to the left side</param>
        ///// <param name="conversionToRight">An action that will convert the value from the left side to the right side</param>
        ///// <param name="updatingWay">The direction where the action will be peformed. If it was set as [Way.None], it should be manually performed manually.</param>
        //public static IBindingPeer BindsToSource<TControl, TSource, TLeftProperty, TRightProperty>(this TControl control, Expression<Func<TControl, TLeftProperty>> controlMember, string controlEventName,
        //                                        TSource dataSource, Expression<Func<TSource, TRightProperty>> dataSourceMember,
        //                                        Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                        UpdatingWay updatingWay = UpdatingWay.Both, Way startUpdating = Way.ToLeft)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    EventHandlerBindingTrigger<TControl> controlTrigger = new EventHandlerBindingTrigger<TControl>(controlEventName, control, ExprenssionHelper.MemberName(controlMember));
        //    PropertyChangedEventHandlerBindingTrigger<TSource> dataTrigger = new PropertyChangedEventHandlerBindingTrigger<TSource>(dataSource, ExprenssionHelper.MemberName(dataSourceMember));

        //    BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(controlTrigger, dataTrigger, conversionToLeft, conversionToRight, updatingWay, startUpdating);

        //    DataBindingRegistry.Register(control.FindForm(), bind);
        //    return bind;
        //}

        ///// <summary>
        ///// Binds a control to a data source.
        ///// </summary>
        //public static IBindingPeer BindsToSource<TControl, TSource, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                    TSource dataSource, Expression<Func<TSource, TProperty>> dataSourceMember,
        //                                    UpdatingWay updatingWay = UpdatingWay.Both, Way startUpdating = Way.ToLeft)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    Func<TProperty, TProperty> conversion = (value) => { return value; };
        //    return BindsToSource<TControl, TSource, TProperty, TProperty>(control, controlMember, controlEventName, dataSource, dataSourceMember, conversion, conversion, updatingWay, startUpdating);
        //}

        ///// <summary>
        ///// Binds a control to a data source.
        ///// </summary>
        //public static IBindingPeer BindsToSourceConversion<TControl, TSource, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                    TSource dataSource, Expression<Func<TSource, TProperty>> dataSourceMember,
        //                                    UpdatingWay updatingWay = UpdatingWay.Both, Way startUpdating = Way.ToLeft, Func<TProperty, TProperty> conversion = null)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    Func<TProperty, TProperty> defaultConversion = (value) => { return value; };
        //    defaultConversion = conversion == null ? defaultConversion : conversion;
        //    return BindsToSource<TControl, TSource, TProperty, TProperty>(control, controlMember, controlEventName, dataSource, dataSourceMember, defaultConversion, defaultConversion, updatingWay, startUpdating);
        //}

        ///// <summary>
        ///// Binds a control to a data source.
        ///// </summary>
        //public static IBindingPeer BindsToSource<TControl, TSource, TLeftProperty, TRightProperty>(this TControl control, Expression<Func<TControl, TLeftProperty>> controlMember, string controlEventName,
        //                                        TSource dataSource, string dataSourceMember,
        //                                        Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                        UpdatingWay updatingWay = UpdatingWay.Both, Way startUpdating = Way.ToLeft)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    EventHandlerBindingTrigger<TControl> controlTrigger = new EventHandlerBindingTrigger<TControl>(controlEventName, control, ExprenssionHelper.MemberName(controlMember));
        //    PropertyChangedEventHandlerBindingTrigger<TSource> dataTrigger = new PropertyChangedEventHandlerBindingTrigger<TSource>(dataSource, dataSourceMember);

        //    BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(controlTrigger, dataTrigger, conversionToLeft, conversionToRight, updatingWay, startUpdating);

        //    DataBindingRegistry.Register(control.FindForm(), bind);
        //    return bind;
        //}

        ///// <summary>
        ///// Binds a control to a data source.
        ///// </summary>
        //public static IBindingPeer BindsToSource<TControl, TSource, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                    TSource dataSource, string dataSourceMember,
        //                                    UpdatingWay updatingWay = UpdatingWay.Both, Way startUpdating = Way.ToLeft)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    Func<TProperty, TProperty> conversion = (value) => { return value; };
        //    return BindsToSource<TControl, TSource, TProperty, TProperty>(control, controlMember, controlEventName, dataSource, dataSourceMember, conversion, conversion, updatingWay, startUpdating);
        //}

        #endregion Control To Data Source

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Source To Control

        ///// <summary>
        ///// Binds a source to a control.
        ///// </summary>
        //public static IBindingPeer BindsToControl<TSource, TControl, TLeftProperty, TRightProperty>(this TSource dataSource, Expression<Func<TSource, TRightProperty>> dataSourceMember,
        //                                                                                            TControl control, Expression<Func<TControl, TLeftProperty>> controlMember, string controlEventName,
        //                                                                                            Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                                                                            UpdatingWay updatingWay = UpdatingWay.Both)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    //EventHandlerBindingTrigger<TControl, TLeftProperty> controlTrigger = new EventHandlerBindingTrigger<TControl, TLeftProperty>(controlEventName, control, ExprenssionHelper.Name(controlMember));
        //    //PropertyChangedEventHandlerBindingTrigger<TSource, TRightProperty> dataTrigger = new PropertyChangedEventHandlerBindingTrigger<TSource, TRightProperty>(dataSource, ExprenssionHelper.Name(dataSourceMember));

        //    EventHandlerBindingTrigger<TControl> controlTrigger = new EventHandlerBindingTrigger<TControl>(controlEventName, control, ExprenssionHelper.MemberName(controlMember));
        //    PropertyChangedEventHandlerBindingTrigger<TSource> dataTrigger = new PropertyChangedEventHandlerBindingTrigger<TSource>(dataSource, ExprenssionHelper.MemberName(dataSourceMember));

        //    BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(controlTrigger, dataTrigger, conversionToLeft, conversionToRight, updatingWay);

        //    DataBindingRegistry.Register(control.FindForm(), bind);
        //    return bind;
        //}

        ///// <summary>
        ///// Binds a source to a control.
        ///// </summary>
        //public static IBindingPeer BindsToControl<TSource, TControl, TProperty>(this TSource dataSource, Expression<Func<TSource, TProperty>> dataSourceMember,
        //                                                                        TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                                                        UpdatingWay updatingWay = UpdatingWay.Both)
        //    where TSource : class, System.ComponentModel.INotifyPropertyChanged
        //    where TControl : Control
        //{
        //    Func<TProperty, TProperty> conversion = (value) => { return value; };
        //    return BindsToControl<TSource, TControl, TProperty, TProperty>(dataSource, dataSourceMember, control, controlMember, controlEventName, conversion, conversion, updatingWay);
        //}

        #endregion Source To Control

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Source To Source

        /// <summary>
        /// Binds a source to another source.
        /// </summary>
        public static IBindingPeer BindsToSource<TLeftSource, TRightSource, TLeftProperty, TRightProperty>(this TLeftSource leftDataSource, Expression<Func<TLeftSource, TRightProperty>> leftDataSourceMember,
                                                                                                            TRightSource rightDataSource, Expression<Func<TRightSource, TRightProperty>> rightDataSourceMember,
                                                                                                            Func<TRightProperty, TLeftProperty> conversionToLeft, Func<TLeftProperty, TRightProperty> conversionToRight,
                                                                                                            UpdatingWay updatingWay = UpdatingWay.Both)
            where TLeftSource : class, System.ComponentModel.INotifyPropertyChanged
            where TRightSource : class, System.ComponentModel.INotifyPropertyChanged
        {
            PropertyChangedEventHandlerBindingTrigger<TLeftSource> leftDataTrigger = new PropertyChangedEventHandlerBindingTrigger<TLeftSource>(leftDataSource, ExprenssionHelper.MemberName(leftDataSourceMember));
            PropertyChangedEventHandlerBindingTrigger<TRightSource> rightDataTrigger = new PropertyChangedEventHandlerBindingTrigger<TRightSource>(rightDataSource, ExprenssionHelper.MemberName(rightDataSourceMember));

            BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(leftDataTrigger, rightDataTrigger, conversionToLeft, conversionToRight, updatingWay);

            //DataBindingRegistry.Register(control.FindForm(), bind);
            return bind;
        }

        /// <summary>
        /// Binds a source to another source.
        /// </summary>
        public static IBindingPeer BindsToSource<TLeftSource, TRightSource, TProperty>(this TLeftSource leftDataSource, Expression<Func<TLeftSource, TProperty>> leftDataSourceMember,
                                                                                        TRightSource rightDataSource, Expression<Func<TRightSource, TProperty>> rightDataSourceMember,
                                                                                        UpdatingWay updatingWay = UpdatingWay.Both)
            where TLeftSource : class, System.ComponentModel.INotifyPropertyChanged
            where TRightSource : class, System.ComponentModel.INotifyPropertyChanged
        {
            Func<TProperty, TProperty> conversion = (value) => { return value; };
            return BindsToSource<TLeftSource, TRightSource, TProperty, TProperty>(leftDataSource, leftDataSourceMember, rightDataSource, rightDataSourceMember, conversion, conversion, updatingWay);
        }

        /// <summary>
        /// Binds a source to another source.
        /// </summary>
        public static IBindingPeer BindsToSource<TSource, TProperty>(this TSource leftDataSource, TSource rightDataSource,
                                                                        Expression<Func<TSource, TProperty>> dataSourceMember,
                                                                        UpdatingWay updatingWay = UpdatingWay.Both)
            where TSource : class, System.ComponentModel.INotifyPropertyChanged
        {
            Func<TProperty, TProperty> conversion = (value) => { return value; };
            return BindsToSource<TSource, TSource, TProperty, TProperty>(leftDataSource, dataSourceMember, rightDataSource, dataSourceMember, conversion, conversion, updatingWay);
        }

        #endregion Source To Source

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Source To Non Source

        /// <summary>
        /// Binds a source to a non source.
        /// </summary>
        public static IBindingPeer BindsToNonSource<TSource, TNonSource, TLeftProperty, TRightProperty>(this TSource dataSource, Expression<Func<TSource, TRightProperty>> dataSourceMember,
                                                                                                            TNonSource nonDataSource, Expression<Func<TNonSource, TRightProperty>> nonDataSourceMember,
                                                                                                            Func<TLeftProperty, TRightProperty> conversionToRight,
                                                                                                            SingleUpdatingWay singleUpdatingWay = SingleUpdatingWay.LeftToRight)
            where TSource : class, System.ComponentModel.INotifyPropertyChanged
            where TNonSource : class
        {
            //UpdatingWay updatingWay = singleUpdatingWay.ToEnum<UpdatingWay>();
            UpdatingWay updatingWay = (UpdatingWay)Enum.Parse(typeof(UpdatingWay), singleUpdatingWay.ToString());

            PropertyChangedEventHandlerBindingTrigger<TSource> leftDataTrigger = new PropertyChangedEventHandlerBindingTrigger<TSource>(dataSource, ExprenssionHelper.MemberName(dataSourceMember));
            NoEventBindingTrigger rightDataTrigger = new NoEventBindingTrigger(nonDataSource, ExprenssionHelper.MemberName(nonDataSourceMember));

            Func<TRightProperty, TLeftProperty> conversionToLeft = null;

            BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(leftDataTrigger, rightDataTrigger, conversionToLeft, conversionToRight, updatingWay);

            //DataBindingRegistry.Register(control.FindForm(), bind);
            return bind;
        }

        /// <summary>
        /// Binds a source to a non source.
        /// </summary>
        public static IBindingPeer BindsToNonSource<TSource, TNonSource, TProperty>(this TSource dataSource, Expression<Func<TSource, TProperty>> dataSourceMember,
                                                                                        TNonSource nonDataSource, Expression<Func<TNonSource, TProperty>> nonDataSourceMember,
                                                                                        SingleUpdatingWay singleUpdatingWay = SingleUpdatingWay.LeftToRight)
            where TSource : class, System.ComponentModel.INotifyPropertyChanged
            where TNonSource : class
        {
            Func<TProperty, TProperty> conversion = (value) => { return value; };
            return BindsToNonSource<TSource, TNonSource, TProperty, TProperty>(dataSource, dataSourceMember, nonDataSource, nonDataSourceMember, conversion, singleUpdatingWay);
        }

        #endregion Source To Non Source

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        #region Control To Non Source

        ///// <summary>
        ///// Binds a control to a non source.
        ///// </summary>
        //public static IBindingPeer BindsToNonSource<TControl, TNonSource, TLeftProperty, TRightProperty>(this TControl control, Expression<Func<TControl, TLeftProperty>> controlMember, string controlEventName,
        //                                                                                                    TNonSource nonDataSource, Expression<Func<TNonSource, TRightProperty>> nonDataSourceMember,
        //                                                                                                    Func<TLeftProperty, TRightProperty> conversionToRight,
        //                                                                                                    SingleUpdatingWay singleUpdatingWay = SingleUpdatingWay.LeftToRight)
        //    where TControl : System.Windows.Forms.Control
        //    where TNonSource : class
        //{

        //    //UpdatingWay updatingWay = singleUpdatingWay.ToEnum<UpdatingWay>();
        //    UpdatingWay updatingWay = (UpdatingWay)Enum.Parse(typeof(UpdatingWay), singleUpdatingWay.ToString());

        //    EventHandlerBindingTrigger<TControl> leftDataTrigger = new EventHandlerBindingTrigger<TControl>(controlEventName, control, ExprenssionHelper.MemberName(controlMember));
        //    NoEventBindingTrigger rightDataTrigger = new NoEventBindingTrigger(nonDataSource, ExprenssionHelper.MemberName(nonDataSourceMember));

        //    BindingPeer<TLeftProperty, TRightProperty> bind = new BindingPeer<TLeftProperty, TRightProperty>(leftDataTrigger, rightDataTrigger, null, conversionToRight, updatingWay);

        //    //DataBindingRegistry.Register(control.FindForm(), bind);
        //    return bind;
        //}

        ///// <summary>
        ///// Binds a control to a non source.
        ///// </summary>
        //public static IBindingPeer BindsToNonSource<TControl, TNonSource, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> controlMember, string controlEventName,
        //                                                                                TNonSource nonDataSource, Expression<Func<TNonSource, TProperty>> nonDataSourceMember,
        //                                                                                SingleUpdatingWay singleUpdatingWay = SingleUpdatingWay.LeftToRight)
        //    where TControl : System.Windows.Forms.Control
        //    where TNonSource : class
        //{
        //    Func<TProperty, TProperty> conversion = (value) => { return value; };
        //    return BindsToNonSource<TControl, TNonSource, TProperty, TProperty>(control, controlMember, controlEventName, nonDataSource, nonDataSourceMember, conversion, singleUpdatingWay);
        //}

        #endregion Control To Non Source

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    }
}