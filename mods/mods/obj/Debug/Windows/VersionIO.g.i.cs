﻿#pragma checksum "..\..\..\Windows\VersionIO.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "CE46F4C357C941940DE058557D17B6B000FD1C1D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using mods;


namespace mods {
    
    
    /// <summary>
    /// VersionIO
    /// </summary>
    public partial class VersionIO : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox Filter;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabItem Inputs;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.WrapPanel InputsSP;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabItem Outputs;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.WrapPanel OutputsSP;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Windows\VersionIO.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SubmitButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/mods;component/windows/versionio.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\VersionIO.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Filter = ((System.Windows.Controls.TextBox)(target));
            
            #line 10 "..\..\..\Windows\VersionIO.xaml"
            this.Filter.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.Filter_TextChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Inputs = ((System.Windows.Controls.TabItem)(target));
            return;
            case 3:
            this.InputsSP = ((System.Windows.Controls.WrapPanel)(target));
            return;
            case 4:
            this.Outputs = ((System.Windows.Controls.TabItem)(target));
            return;
            case 5:
            this.OutputsSP = ((System.Windows.Controls.WrapPanel)(target));
            return;
            case 6:
            this.SubmitButton = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\Windows\VersionIO.xaml"
            this.SubmitButton.Click += new System.Windows.RoutedEventHandler(this.SubmitButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
