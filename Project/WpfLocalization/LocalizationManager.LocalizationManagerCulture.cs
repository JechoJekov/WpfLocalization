using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace WpfLocalization
{
    partial class LocalizationManager
    {
        /// <summary>
        /// Provides binding support for the static *Culture properties of the <see cref="LocalizationManager"/> type.
        /// </summary>
        public class LocalizationManagerCulture : INotifyPropertyChanged
        {
            [ThreadStatic]
            PropertyChangedEventHandler _propertyChanged;

            /// <remarks>
            /// CAUTION This even is fired only on a UI thread that controls at least one localized value.
            /// </remarks>
            /// <exception cref="InvalidOperationException">The event is accessed from a non-UI thread.</exception>
            public event PropertyChangedEventHandler PropertyChanged
            {
                add
                {
                    WpfUtils.VerifyIsUIThread();

                    _propertyChanged += value;
                }
                remove
                {
                    WpfUtils.VerifyIsUIThread();

                    _propertyChanged -= value;
                }
            }

            internal LocalizationManagerCulture() { }

            /// <summary>
            /// Asynchronously fires the <see cref="PropertyChanged"/> event on the specified <see cref="Dispatcher"/>.
            /// </summary>
            /// <param name="dispatcher"></param>
            /// <param name="propertyName"></param>
            internal void FirePropertyChangedAsync(Dispatcher dispartcher, string propertyName)
            {
                Debug.Assert(dispartcher != null);

                dispartcher.BeginInvoke(DispatcherPriority.Normal, new SendOrPostCallback(FirePropertyChanged), propertyName);
            }

            internal void FirePropertyChanged(string propertyName)
            {
                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            void FirePropertyChanged(object propertyName)
            {
                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs((string)propertyName));
            }

            /// <summary>
            /// Gets or sets <see cref="Thread.CurrentCulture"/> of the current UI thread.
            /// </summary>
            /// <remarks>
            /// <para>
            /// <see cref="Thread.CurrentCulture"/> is used to format dates and numbers.
            /// </para>
            /// <para>
            /// CAUTION This method will set the culture only on the current UI thread. To set the culture of all UI threads (in case the application uses more than one)
            /// use <see cref="SetGlobalCulture"/>.
            /// </para>
            /// </remarks>
            /// <exception cref="InvalidOperationException">The current thread is not a UI thread.</exception>
            public CultureInfo CurrentCulture
            {
                get
                {
                    return LocalizationManager.CurrentCulture;
                }
                set
                {
                    LocalizationManager.CurrentCulture = value;
                }
            }

            /// <summary>
            /// Gets or sets <see cref="Thread.CurrentUICulture"/> of the current UI thread.
            /// </summary>
            /// <remarks>
            /// <para>
            /// <see cref="Thread.CurrentUICulture"/> specifies the culture of the resources (e.g. text and images).
            /// </para>
            /// <para>
            /// CAUTION This method will set the culture only on the current UI thread. To set the culture of all UI threads (in case the application uses more than one)
            /// use <see cref="GlobalCulture"/>.
            /// </para>
            /// </remarks>
            /// <exception cref="InvalidOperationException">The current thread is not a UI thread.</exception>
            public CultureInfo CurrentUICulture
            {
                get
                {
                    return LocalizationManager.CurrentUICulture;
                }
                set
                {
                    LocalizationManager.CurrentUICulture = value;
                }
            }

            /// <summary>
            /// Gets or sets <see cref="Thread.CurrentCulture"/> of *all* UI threads that at least one localized property.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This property is most useful for applications that utilize multiple UI threads. For applications that have only a single UI thread
            /// the <see cref="CurrentCulture"/> provides a bit better performance. However, this method can be called from any thread and not just a UI thread
            /// which can be useful for single-threaded applications as well.
            /// </para>
            /// <para>
            /// The initial value of this property is the culture of the primary UI thread (<see cref="Application.Current"/>).
            /// </para>
            /// <para>
            /// While the setter sets the culture of *all* UI threads the getter returns either the culture of the primary UI thread (the initial value)
            /// or the last value set . There is no guarantee that the <see cref="CultureInfo"/> by the getter is the actual culture of every
            /// UI thread as each UI thread has its own culture setting.
            /// </para>
            /// <para>
            /// The setter sets the culture of all UI threads that controls at least one localized value. Threads that do not use localization
            /// are unknown to <see cref="LocalizationManager"/>.
            /// </para>
            /// <para>
            /// The value of this property is used by the <see cref="SpawnUIThread"/> method to initialize the culture of the newly spawned UI thread.
            /// </para>
            /// <para>
            /// <see cref="Thread.CurrentCulture"/> is used to format dates and numbers.
            /// </para>
            /// </remarks>
            /// <exception cref="InvalidOperationException">The getter is accessed in a non-WPF application.</exception>
            public CultureInfo GlobalCulture
            {
                get
                {
                    return LocalizationManager.GlobalCulture;
                }
                set
                {
                    LocalizationManager.GlobalCulture = value;
                }
            }

            /// <summary>
            /// Gets or sets <see cref="Thread.CurrentUICulture"/> of *all* UI threads that at least one localized property.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This property is most useful for applications that utilize multiple UI threads. For applications that have only a single UI thread
            /// the <see cref="CurrentUICulture"/> provides a bit better performance. However, this method can be called from any thread and not just a UI thread
            /// which can be useful for single-threaded applications as well.
            /// </para>
            /// <para>
            /// The initial value of this property is the culture of the primary UI thread (<see cref="Application.Current"/>).
            /// </para>
            /// <para>
            /// While the setter sets the culture of *all* UI threads the getter returns either the culture of the primary UI thread (the initial value)
            /// or the last value set . There is no guarantee that the <see cref="CultureInfo"/> by the getter is the actual culture of every
            /// UI thread as each UI thread has its own culture setting.
            /// </para>
            /// <para>
            /// The setter sets the culture of all UI threads that controls at least one localized value. Threads that do not use localization
            /// are unknown to <see cref="LocalizationManager"/>.
            /// </para>
            /// <para>
            /// The value of this property is used by the <see cref="SpawnUIThread"/> method to initialize the culture of the newly spawned UI thread.
            /// </para>
            /// <para>
            /// <see cref="Thread.CurrentUICulture"/> specifies the culture of the resources (e.g. text and images).
            /// </para>
            /// </remarks>
            /// <exception cref="InvalidOperationException">The getter is accessed in a non-WPF application.</exception>
            public CultureInfo GlobalUICulture
            {
                get
                {
                    return LocalizationManager.GlobalUICulture;
                }
                set
                {
                    LocalizationManager.GlobalUICulture = value;
                }
            }
        }

    }
}
