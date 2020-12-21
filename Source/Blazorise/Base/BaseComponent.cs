﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;
#endregion

namespace Blazorise
{
    public abstract class BaseComponent : ComponentBase, IDisposable
    {
        #region Members

        private string elementId;

        private string customClass;

        private string customStyle;

        private Float @float = Float.None;

        private IFluentSpacing margin;

        private IFluentSpacing padding;

        private IFluentDisplay display;

        private CharacterCasing characterCasing = CharacterCasing.Normal;

        /// <summary>
        /// A stack of functions to execute after the rendering.
        /// </summary>
        private Queue<Func<Task>> executeAfterRendereQueue;

        #endregion

        #region Constructors

        public BaseComponent()
        {
            ClassBuilder = new ClassBuilder( BuildClasses );
            StyleBuilder = new StyleBuilder( BuildStyles );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose( true );
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="BaseComponent"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True if the component is in the disposing process.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !Disposed )
            {
                Disposed = true;
            }
        }

        /// <summary>
        /// Pushes an action to the stack to be executed after the rendering is done.
        /// </summary>
        /// <param name="action"></param>
        protected void ExecuteAfterRender( Func<Task> action )
        {
            if ( executeAfterRendereQueue == null )
                executeAfterRendereQueue = new Queue<Func<Task>>();

            executeAfterRendereQueue.Enqueue( action );
        }

        protected override async Task OnAfterRenderAsync( bool firstRender )
        {
            Rendered = true;

            if ( firstRender )
            {
                await OnFirstAfterRenderAsync();
            }

            if ( executeAfterRendereQueue?.Count > 0 )
            {
                var actions = executeAfterRendereQueue.ToArray();
                executeAfterRendereQueue.Clear();

                foreach ( var action in actions )
                {
                    await action();
                }
            }

            await base.OnAfterRenderAsync( firstRender );
        }

        /// <summary>
        /// Method is called only once when component is first rendered.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnFirstAfterRenderAsync()
            => Task.CompletedTask;

        /// <summary>
        /// Builds a list of classnames for this component.
        /// </summary>
        /// <param name="builder">Class builder used to append the classnames.</param>
        protected virtual void BuildClasses( ClassBuilder builder )
        {
            if ( Class != null )
                builder.Append( Class );

            if ( Margin != null )
                builder.Append( Margin.Class( ClassProvider ) );

            if ( Padding != null )
                builder.Append( Padding.Class( ClassProvider ) );

            if ( Display != null )
                builder.Append( Display.Class( ClassProvider ) );

            if ( Float != Float.None )
                builder.Append( ClassProvider.ToFloat( Float ) );

            if ( Casing != CharacterCasing.Normal )
                builder.Append( ClassProvider.Casing( Casing ) );
        }

        /// <summary>
        /// Builds a list of styles for this component.
        /// </summary>
        /// <param name="builder">Style builder used to append the styles.</param>
        protected virtual void BuildStyles( StyleBuilder builder )
        {
            if ( Style != null )
                builder.Append( Style );
        }

        /// <summary>
        /// Clears the class-names and mark them to be regenerated.
        /// </summary>
        internal protected virtual void DirtyClasses()
        {
            ClassBuilder.Dirty();
        }

        /// <summary>
        /// Clears the styles-names and mark them to be regenerated.
        /// </summary>
        protected virtual void DirtyStyles()
        {
            StyleBuilder.Dirty();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Flag that indicates if the component is already fully disposed.
        /// </summary>
        protected bool Disposed { get; private set; }

        /// <summary>
        /// Gets or sets the reference to the rendered element.
        /// </summary>
        public ElementReference ElementRef { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the element.
        /// </summary>
        /// <remarks>
        /// Note that this ID is not defined for the component but instead for the underlined element that it represents.
        /// eg: for the TextEdit the ID will be set on the input element.
        /// </remarks>
        [Parameter]
        public string ElementId
        {
            get => elementId ??= IdGenerator.Generate;
            set => elementId = value;
        }

        /// <summary>
        /// Gets the class builder.
        /// </summary>
        protected ClassBuilder ClassBuilder { get; private set; }

        /// <summary>
        /// Gets the built class-names based on all the rules set by the component parameters.
        /// </summary>
        public string ClassNames => ClassBuilder.Class;

        /// <summary>
        /// Indicates if component has been rendered in the browser.
        /// </summary>
        protected bool Rendered { get; private set; }

        /// <summary>
        /// Gets the style mapper.
        /// </summary>
        protected StyleBuilder StyleBuilder { get; private set; }

        /// <summary>
        /// Gets the built styles based on all the rules set by the component parameters.
        /// </summary>
        public string StyleNames => StyleBuilder.Styles;

        /// <summary>
        /// Gets or set the javascript runner.
        /// </summary>
        [Inject] protected IIdGenerator IdGenerator { get; set; }

        /// <summary>
        /// Gets or set the javascript runner.
        /// </summary>
        [Inject] protected IJSRunner JSRunner { get; set; }

        /// <summary>
        /// Gets or sets the classname provider.
        /// </summary>
        [Inject]
        protected IClassProvider ClassProvider { get; set; }

        /// <summary>
        /// Gets or sets the style provider.
        /// </summary>
        [Inject]
        protected IStyleProvider StyleProvider { get; set; }

        /// <summary>
        /// Custom css classname.
        /// </summary>
        [Parameter]
        public string Class
        {
            get => customClass;
            set
            {
                customClass = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Custom html style.
        /// </summary>
        [Parameter]
        public string Style
        {
            get => customStyle;
            set
            {
                customStyle = value;

                DirtyStyles();
            }
        }

        /// <summary>
        /// Floats an element to the defined side.
        /// </summary>
        [Parameter]
        public Float Float
        {
            get => @float;
            set
            {
                @float = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Defines the element margin spacing.
        /// </summary>
        [Parameter]
        public IFluentSpacing Margin
        {
            get => margin;
            set
            {
                margin = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Defines the element padding spacing.
        /// </summary>
        [Parameter]
        public IFluentSpacing Padding
        {
            get => padding;
            set
            {
                padding = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Specifies the display behavior of an element.
        /// </summary>
        [Parameter]
        public IFluentDisplay Display
        {
            get => display;
            set
            {
                display = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Changes the character casing of a element.
        /// </summary>
        [Parameter]
        public CharacterCasing Casing
        {
            get => characterCasing;
            set
            {
                characterCasing = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Captures all the custom attribute that are not part of Blazorise component.
        /// </summary>
        [Parameter( CaptureUnmatchedValues = true )]
        public Dictionary<string, object> Attributes { get; set; }

        #endregion
    }
}