﻿namespace Caliburn.Micro.Xamarin.Forms {

    using System;
    using System.Threading.Tasks;
    using global::Xamarin.Forms;

    /// <summary>
    /// Adapater around NavigationPage that implements INavigationService
    /// </summary>
    public class NavigationPageAdapter : INavigtionService {
        private readonly NavigationPage navigationPage;

        /// <summary>
        /// Instantiates new instance of NavigationPageAdapter
        /// </summary>
        /// <param name="navigationPage">The navigation page to adapat</param>
        public NavigationPageAdapter(NavigationPage navigationPage) {
            this.navigationPage = navigationPage;
        }

        private static void DeactivateView(BindableObject view)
        {
            if (view == null)
                return;

            var deactivate = view.BindingContext as IDeactivate;

            if (deactivate != null)
            {
                deactivate.Deactivate(false);
            }
        }

        private static void ActivateView(BindableObject view)
        {
            if (view == null)
                return;

            var activator = view.BindingContext as IActivate;

            if (activator != null)
            {
                activator.Activate();
            }
        }

        /// <summary>
        /// Asynchronously removes the top <see cref="T:Xamarin.Forms.Page"/> from the navigation stack, with optional animation.
        /// </summary>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PopAsync(bool animated = true)
        {
            return navigationPage.PopAsync(animated);
        }

        /// <summary>
        /// Pops all but the root <see cref="T:Xamarin.Forms.Page"/> off the navigation stack.
        /// </summary>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PopToRootAsync(bool animated = true) 
        {
            return navigationPage.PopToRootAsync(animated);
        }

        /// <summary>
        ///  A task for asynchronously pushing a view for the given view model onto the navigation stack, with optional animation.
        /// </summary>
        /// <param name="viewModelType">The type of the view model</param>
        /// <param name="parameter">The paramter to pass to the view model</param>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PushViewModelAsync(Type viewModelType, object parameter = null, bool animated = true)
        {
            var view = ViewLocator.LocateForModelType(viewModelType, null, null);

            return PushAsync(view, parameter, animated);
        }

        /// <summary>
        ///  A task for asynchronously pushing a page for the given view model onto the navigation stack, with optional animation.
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="parameter">The paramter to pass to the view model</param>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PushViewModelAsync<T>(object parameter = null, bool animated = true)
        {
           return PushViewModelAsync(typeof(T), parameter, animated);
        }

        /// <summary>
        ///  A task for asynchronously pushing a view onto the navigation stack, with optional animation.
        /// </summary>
        /// <param name="viewType">The type of the view</param>
        /// <param name="parameter">The paramter to pass to the view model</param>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PushViewAsync(Type viewType, object parameter = null, bool animated = true)
        {
            var view = ViewLocator.GetOrCreateViewType(viewType);

            return PushAsync(view, parameter, animated);
        }

        /// <summary>
        ///  A task for asynchronously pushing a view onto the navigation stack, with optional animation.
        /// </summary>
        /// <typeparam name="T">The type of the view</typeparam>
        /// <param name="parameter">The paramter to pass to the view model</param>
        /// <param name="animated">Animate the transition</param>
        /// <returns>The asynchrous task representing the transition</returns>
        public Task PushViewAsync<T>(object parameter = null, bool animated = true)
        {
            return PushViewAsync(typeof(T), parameter, animated);
        }

        private Task PushAsync(Element view, object parameter, bool animated)
        {
            var page = view as Page;

            if (page == null)
                throw new NotSupportedException(String.Format("{0} does not inherit from {1}.", view.GetType(), typeof(Page)));

            var viewModel = ViewModelLocator.LocateForView(view);

            if (viewModel != null) {
                TryInjectParameters(viewModel, parameter);

                ViewModelBinder.Bind(viewModel, view, null);
            }

            page.Appearing += (s, e) => ActivateView(page);
            page.Disappearing += (s, e) => DeactivateView(page);

            return navigationPage.PushAsync(page, animated);
        }

        /// <summary>
        /// Attempts to inject query string parameters from the view into the view model.
        /// </summary>
        /// <param name="viewModel"> The view model.</param>
        /// <param name="parameter"> The parameter.</param>
        protected virtual void TryInjectParameters(object viewModel, object parameter)
        {
            var viewModelType = viewModel.GetType();

            if (parameter is string && ((string)parameter).StartsWith("caliburn://"))
            {
                var uri = new Uri((string)parameter);

                if (!String.IsNullOrEmpty(uri.Query)) {

                    var decorder = HttpUtility.ParseQueryString(uri.Query);

                    foreach (var pair in decorder)
                    {
                        var property = viewModelType.GetPropertyCaseInsensitive(pair.Key);

                        if (property == null)
                        {
                            continue;
                        }

                        property.SetValue(viewModel, MessageBinder.CoerceValue(property.PropertyType, pair.Value, null));
                    }
                }
            }
            else
            {
                var property = viewModelType.GetPropertyCaseInsensitive("Parameter");

                if (property == null)
                    return;

                property.SetValue(viewModel, MessageBinder.CoerceValue(property.PropertyType, parameter, null));
            }
        }
    }
}
