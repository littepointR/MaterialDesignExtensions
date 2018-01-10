﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;

using MaterialDesignExtensions.Controllers;
using MaterialDesignExtensions.Model;

namespace MaterialDesignExtensions.Controls
{
    /// <summary>
    /// A control which implements the Stepper of the Material design specification (https://material.google.com/components/steppers.html).
    /// </summary>
    [ContentProperty(nameof(Steps))]
    public class Stepper : Control
    {
        public static RoutedCommand BackCommand = new RoutedCommand();
        public static RoutedCommand CancelCommand = new RoutedCommand();
        public static RoutedCommand ContinueCommand = new RoutedCommand();
        public static RoutedCommand StepSelectedCommand = new RoutedCommand();

        /// <summary>
        /// An event raised by changing to active <see cref="IStep" />.
        /// </summary>
        public static readonly RoutedEvent ActiveStepChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ActiveStepChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by changing to active <see cref="IStep" />.
        /// </summary>
        public event RoutedEventHandler ActiveStepChanged
        {
            add
            {
                AddHandler(ActiveStepChangedEvent, value);
            }

            remove
            {
                RemoveHandler(ActiveStepChangedEvent, value);
            }
        }

        /// <summary>
        /// An event raised by navigating to the previous <see cref="IStep" /> in a linear order.
        /// </summary>
        public static readonly RoutedEvent BackNavigationEvent = EventManager.RegisterRoutedEvent(
            nameof(BackNavigation), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by navigating to the previous <see cref="IStep" /> in a linear order.
        /// </summary>
        public event RoutedEventHandler BackNavigation
        {
            add
            {
                AddHandler(BackNavigationEvent, value);
            }

            remove
            {
                RemoveHandler(BackNavigationEvent, value);
            }
        }

        /// <summary>
        /// An event raised by cancelling the process.
        /// </summary>
        public static readonly RoutedEvent CancelNavigationEvent = EventManager.RegisterRoutedEvent(
            nameof(CancelNavigation), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by cancelling the process.
        /// </summary>
        public event RoutedEventHandler CancelNavigation
        {
            add
            {
                AddHandler(CancelNavigationEvent, value);
            }

            remove
            {
                RemoveHandler(CancelNavigationEvent, value);
            }
        }

        /// <summary>
        /// An event raised by navigating to the next <see cref="IStep" /> in a linear order.
        /// </summary>
        public static readonly RoutedEvent ContinueNavigationEvent = EventManager.RegisterRoutedEvent(
            nameof(ContinueNavigation), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by navigating to the next <see cref="IStep" /> in a linear order.
        /// </summary>
        public event RoutedEventHandler ContinueNavigation
        {
            add
            {
                AddHandler(ContinueNavigationEvent, value);
            }

            remove
            {
                RemoveHandler(ContinueNavigationEvent, value);
            }
        }

        /// <summary>
        /// An event raised by navigating to an arbitrary <see cref="IStep" /> in a non-linear <see cref="Stepper" />.
        /// </summary>
        public static readonly RoutedEvent StepNavigationEvent = EventManager.RegisterRoutedEvent(
            nameof(StepNavigation), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by navigating to an arbitrary <see cref="IStep" /> in a non-linear <see cref="Stepper" />.
        /// </summary>
        public event RoutedEventHandler StepNavigation
        {
            add
            {
                AddHandler(StepNavigationEvent, value);
            }

            remove
            {
                RemoveHandler(StepNavigationEvent, value);
            }
        }

        /// <summary>
        /// An event raised by starting the validation of an <see cref="IStep" />.
        /// </summary>
        public static readonly RoutedEvent StepValidationEvent = EventManager.RegisterRoutedEvent(
            nameof(StepValidation), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

        /// <summary>
        /// An event raised by starting the validation of an <see cref="IStep" />.
        /// </summary>
        public event RoutedEventHandler StepValidation
        {
            add
            {
                AddHandler(StepValidationEvent, value);
            }

            remove
            {
                RemoveHandler(StepValidationEvent, value);
            }
        }

        /// <summary>
        /// The active <see cref="IStep" />. (read-only)
        /// </summary>
        public static readonly DependencyPropertyKey ActiveStepPropertyKey = DependencyProperty.RegisterReadOnly(
                nameof(ActiveStep), typeof(IStep), typeof(Stepper), new PropertyMetadata(null, null));

        /// <summary>
        /// The active <see cref="IStep" />. (read-only)
        /// </summary>
        public static readonly DependencyProperty ActiveStepProperty = ActiveStepPropertyKey.DependencyProperty;

        /// <summary>
        /// The active <see cref="IStep" />. (read-only)
        /// </summary>
        public IStep ActiveStep
        {
            get
            {
                return (IStep)GetValue(ActiveStepProperty);
            }

            private set
            {
                SetValue(ActiveStepPropertyKey, value);
            }
        }

        /// <summary>
        /// A command called by changing the active <see cref="IStep" />.
        /// </summary>
        public static readonly DependencyProperty ActiveStepChangedCommandProperty = DependencyProperty.Register(
            nameof(ActiveStepChangedCommand), typeof(ICommand), typeof(Stepper), new PropertyMetadata(null, null));

        /// <summary>
        /// A command called by changing the active <see cref="IStep" />.
        /// </summary>
        public ICommand ActiveStepChangedCommand
        {
            get
            {
                return (ICommand)GetValue(ActiveStepChangedCommandProperty);
            }

            set
            {
                SetValue(ActiveStepChangedCommandProperty, value);
            }
        }

        /// <summary>
        /// Specifies whether validation errors will block the navigation or not.
        /// </summary>
        public static readonly DependencyProperty BlockNavigationOnValidationErrorsProperty = DependencyProperty.Register(
                nameof(BlockNavigationOnValidationErrors), typeof(bool), typeof(Stepper), new PropertyMetadata(false));

        /// <summary>
        /// Specifies whether validation errors will block the navigation or not.
        /// </summary>
        public bool BlockNavigationOnValidationErrors
        {
            get
            {
                return (bool)GetValue(BlockNavigationOnValidationErrorsProperty);
            }

            set
            {
                SetValue(BlockNavigationOnValidationErrorsProperty, value);
            }
        }

        /// <summary>
        /// Enables the linear mode by disabling the buttons of the header.
        /// The navigation must be accomplished by using the navigation commands.
        /// </summary>
        public static readonly DependencyProperty IsLinearProperty = DependencyProperty.Register(
                nameof(IsLinear), typeof(bool), typeof(Stepper), new PropertyMetadata(false));

        /// <summary>
        /// Enables the linear mode by disabling the buttons of the header.
        /// The navigation must be accomplished by using the navigation commands.
        /// </summary>
        public bool IsLinear
        {
            get
            {
                return (bool)GetValue(IsLinearProperty);
            }

            set
            {
                SetValue(IsLinearProperty, value);
            }
        }

        /// <summary>
        /// Defines this <see cref="Stepper" /> as either horizontal or vertical.
        /// </summary>
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
                nameof(Layout), typeof(StepperLayout), typeof(Stepper), new PropertyMetadata(StepperLayout.Horizontal));

        /// <summary>
        /// Defines this <see cref="Stepper" /> as either horizontal or vertical.
        /// </summary>
        public StepperLayout Layout
        {
            get
            {
                return (StepperLayout)GetValue(LayoutProperty);
            }

            set
            {
                SetValue(LayoutProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the steps which will be shown inside this <see cref="Stepper" />.
        /// </summary>
        public static readonly DependencyProperty StepsProperty = DependencyProperty.Register(
                nameof(Steps), typeof(IList), typeof(Stepper), new PropertyMetadata(new ObservableCollection<IStep>(), StepsChangedHandler));

        /// <summary>
        /// Gets or sets the steps which will be shown inside this <see cref="Stepper" />.
        /// </summary>
        public IList Steps
        {
            get
            {
                return (IList)GetValue(StepsProperty);
            }

            set
            {
                SetValue(StepsProperty, value);
            }
        }

        /// <summary>
        /// A command called by starting the validation of an <see cref="IStep" />.
        /// </summary>
        public static readonly DependencyProperty StepValidationCommandProperty = DependencyProperty.Register(
            nameof(StepValidationCommand), typeof(ICommand), typeof(Stepper), new PropertyMetadata(null, null));

        /// <summary>
        /// A command called by starting the validation of an <see cref="IStep" />.
        /// </summary>
        public ICommand StepValidationCommand
        {
            get
            {
                return (ICommand)GetValue(StepValidationCommandProperty);
            }

            set
            {
                SetValue(StepValidationCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets the controller for this <see cref="Stepper" />.
        /// </summary>
        public StepperController Controller
        {
            get
            {
                return m_controller;
            }
        }

        private StepperController m_controller;

        static Stepper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Stepper), new FrameworkPropertyMetadata(typeof(Stepper)));
        }

        /// <summary>
        /// Creates a new <see cref="Stepper" />.
        /// </summary>
        public Stepper()
            : base()
        {
            m_controller = new StepperController();

            Loaded += LoadedHandler;
            Unloaded += UnloadedHandler;

            CommandBindings.Add(new CommandBinding(BackCommand, BackHandler));
            CommandBindings.Add(new CommandBinding(CancelCommand, CancelHandler));
            CommandBindings.Add(new CommandBinding(ContinueCommand, ContinueHandler));
            CommandBindings.Add(new CommandBinding(StepSelectedCommand, StepSelectedHandler, CanExecuteStepSelectedHandler));
        }

        private void LoadedHandler(object sender, RoutedEventArgs args)
        {
            m_controller.PropertyChanged += PropertyChangedHandler;

            if (Steps is ObservableCollection<IStep> steps)
            {
                steps.CollectionChanged -= StepsCollectionChanged;
                steps.CollectionChanged += StepsCollectionChanged;
            }

            InitSteps(Steps);

            // there is no event raised if the Content of a ContentControl changes
            //     therefore trigger the animation in code
            PlayHorizontalContentAnimation();
        }

        private void UnloadedHandler(object sender, RoutedEventArgs args)
        {
            m_controller.PropertyChanged -= PropertyChangedHandler;

            if (Steps is ObservableCollection<IStep> steps)
            {
                steps.CollectionChanged -= StepsCollectionChanged;
            }
        }

        private bool ValidateActiveStep()
        {
            IStep step = m_controller.ActiveStepViewModel?.Step;

            if (step != null)
            {
                // call the validation method on the step itself
                step.Validate();

                // raise the event and call the command
                StepValidationEventArgs eventArgs = new StepValidationEventArgs(StepValidationEvent, this, step);
                RaiseEvent(eventArgs);

                if (StepValidationCommand != null && StepValidationCommand.CanExecute(step))
                {
                    StepValidationCommand.Execute(step);
                }

                // the event handlers can set the validation state on the step
                return !step.HasValidationErrors;
            }
            else
            {
                // no active step to validate
                return true;
            }
        }

        private static void StepsChangedHandler(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Stepper)?.StepsChangedHandler(args.OldValue, args.NewValue);
        }

        private void StepsChangedHandler(object oldValue, object newValue)
        {
            if (oldValue is ObservableCollection<IStep> oldSteps)
            {
                oldSteps.CollectionChanged -= StepsCollectionChanged;
            }

            if (newValue is ObservableCollection<IStep> newSteps)
            {
                newSteps.CollectionChanged += StepsCollectionChanged;
            }

            InitSteps(newValue as IList);
        }

        private void StepsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            InitSteps(Steps);
        }

        private void InitSteps(IList values)
        {
            IList<IStep> steps = new List<IStep>();

            if (values != null)
            {
                foreach (object item in values)
                {
                    if (item is IStep step)
                    {
                        steps.Add(step);
                    }
                }
            }

            m_controller.InitSteps(steps);
        }

        private void BackHandler(object sender, ExecutedRoutedEventArgs args)
        {
            bool isValid = ValidateActiveStep();

            if (BlockNavigationOnValidationErrors && !isValid)
            {
                return;
            }

            StepperNavigationEventArgs navigationArgs = new StepperNavigationEventArgs(BackNavigationEvent, this, m_controller.ActiveStep, m_controller.PreviousStep, false);
            RaiseEvent(navigationArgs);

            if (!navigationArgs.Cancel)
            {
                m_controller.Back();
            }
        }

        private void CancelHandler(object sender, ExecutedRoutedEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            StepperNavigationEventArgs navigationArgs = new StepperNavigationEventArgs(CancelNavigationEvent, this, m_controller.ActiveStep, null, false);
            RaiseEvent(navigationArgs);
        }

        private void ContinueHandler(object sender, ExecutedRoutedEventArgs args)
        {
            bool isValid = ValidateActiveStep();

            if (BlockNavigationOnValidationErrors && !isValid)
            {
                return;
            }

            StepperNavigationEventArgs navigationArgs = new StepperNavigationEventArgs(ContinueNavigationEvent, this, m_controller.ActiveStep, m_controller.NextStep, false);
            RaiseEvent(navigationArgs);

            if (!navigationArgs.Cancel)
            {
                m_controller.Continue();
            }
        }

        private void CanExecuteStepSelectedHandler(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = !IsLinear;
        }

        private void StepSelectedHandler(object sender, ExecutedRoutedEventArgs args)
        {
            if (!IsLinear)
            {
                bool isValid = ValidateActiveStep();

                if (BlockNavigationOnValidationErrors && !isValid)
                {
                    return;
                }

                StepperNavigationEventArgs navigationArgs = new StepperNavigationEventArgs(StepNavigationEvent, this, m_controller.ActiveStep, ((StepperStepViewModel)args.Parameter).Step, false);
                RaiseEvent(navigationArgs);

                if (!navigationArgs.Cancel)
                {
                    m_controller.GotoStep((StepperStepViewModel)args.Parameter);
                }
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (sender == m_controller)
            {
                if (args.PropertyName == nameof(m_controller.ActiveStep))
                {
                    // set the property
                    ActiveStep = m_controller.ActiveStep;

                    // raise the event and call the command
                    ActiveStepChangedEventArgs eventArgs = new ActiveStepChangedEventArgs(StepValidationEvent, this, ActiveStep);
                    RaiseEvent(eventArgs);

                    if (ActiveStepChangedCommand != null && ActiveStepChangedCommand.CanExecute(ActiveStep))
                    {
                        ActiveStepChangedCommand.Execute(ActiveStep);
                    }
                }
                else if (args.PropertyName == nameof(m_controller.ActiveStepContent)
                    && m_controller.ActiveStepContent != null
                    && Layout == StepperLayout.Horizontal)
                {
                    // there is no event raised if the Content of a ContentControl changes
                    //     therefore trigger the animation in code
                    PlayHorizontalContentAnimation();
                }
            }
        }

        private void PlayHorizontalContentAnimation()
        {
            // there is no event raised if the Content of a ContentControl changes
            //     therefore trigger the animation in code
            if (Layout == StepperLayout.Horizontal)
            {
                Storyboard storyboard = (Storyboard)FindResource("horizontalContentChangedStoryboard");
                FrameworkElement element = GetTemplateChild("PART_horizontalContent") as FrameworkElement;

                if (storyboard != null && element != null)
                {
                    storyboard.Begin(element);
                }
            }
        }
    }

    /// <summary>
    /// The layout of a <see cref="Stepper" />.
    /// </summary>
    public enum StepperLayout : byte
    {
        /// <summary>
        /// Horizontal stepper layout
        /// </summary>
        Horizontal,

        /// <summary>
        /// Vertical stepper layout
        /// </summary>
        Vertical
    }

    /// <summary>
    /// The argument for the <see cref="Stepper.ActiveStepChanged" /> event and the <see cref="Stepper.ActiveStepChangedCommand" /> command.
    /// It holds the new active <see cref="IStep" />.
    /// </summary>
    public class ActiveStepChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The new active <see cref="IStep" />.
        /// </summary>
        public IStep Step { get; }

        /// <summary>
        /// Creates a new <see cref="ActiveStepChangedEventArgs" />.
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name="source"></param>
        /// <param name="step"></param>
        public ActiveStepChangedEventArgs(RoutedEvent routedEvent, object source, IStep step)
            : base(routedEvent, source)
        {
            Step = step;
        }
    }

    /// <summary>
    /// The argument for the <see cref="Stepper.StepValidation" /> event and the <see cref="Stepper.StepValidationCommand" /> command.
    /// It holds the <see cref="IStep" /> to validate.
    /// </summary>
    public class StepValidationEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="IStep" /> to validate.
        /// </summary>
        public IStep Step { get; }

        /// <summary>
        /// Creates a new <see cref="StepValidationEventArgs" />
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name="source"></param>
        /// <param name="step"></param>
        public StepValidationEventArgs(RoutedEvent routedEvent, object source, IStep step)
            : base(routedEvent, source)
        {
            Step = step;
        }
    }

    /// <summary>
    /// The argument for the <see cref="Stepper.BackNavigation" />, <see cref="Stepper.ContinueNavigation" />, <see cref="Stepper.StepNavigation" /> and <see cref="Stepper.CancelNavigation" /> event.
    /// It holds the current <see cref="IStep" /> an the one to navigate to.
    /// The events are raised before the actal navigation and the navigation can be cancelled by setting <see cref="Stepper.ContinueNavigation" /> to false.
    /// </summary>
    public class StepperNavigationEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The current <see cref="IStep" /> of the <see cref="Stepper" />.
        /// </summary>
        public IStep CurrentStep { get; }

        /// <summary>
        /// The next <see cref="IStep" /> to navigate to.
        /// </summary>
        public IStep NextStep { get; }

        /// <summary>
        /// A flag to cancel the navigation by setting it to false.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Creates a new <see cref="StepperNavigationEventArgs" />.
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name="source"></param>
        /// <param name="currentStep"></param>
        /// <param name="nextStep"></param>
        /// <param name="cancel"></param>
        public StepperNavigationEventArgs(RoutedEvent routedEvent, object source, IStep currentStep, IStep nextStep, bool cancel)
            : base(routedEvent, source)
        {
            CurrentStep = currentStep;
            NextStep = nextStep;
            Cancel = cancel;
        }
    }
}
