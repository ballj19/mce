﻿#pragma checksum "..\..\Procedure.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7D509F47C7609BC94AB41C6318E72D5F26AB4FD0"
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
    /// Procedure
    /// </summary>
    public partial class Procedure : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CursorPos;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CursorPosition;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Simulate;
        
        #line default
        #line hidden
        
        /// <summary>
        /// ProcedureLog Name Field
        /// </summary>
        
        #line 13 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBlock ProcedureLog;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CarCallsBox;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox FireServiceBox;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\Procedure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox DLMBox;
        
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
            System.Uri resourceLocater = new System.Uri("/mods;component/procedure.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Procedure.xaml"
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
            this.CursorPos = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\Procedure.xaml"
            this.CursorPos.Click += new System.Windows.RoutedEventHandler(this.CursorPos_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.CursorPosition = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.Simulate = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\Procedure.xaml"
            this.Simulate.Click += new System.Windows.RoutedEventHandler(this.Simulate_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ProcedureLog = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.CarCallsBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.FireServiceBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 7:
            this.DLMBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

