﻿#pragma checksum "..\..\..\LogSign.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "12E2F6A89FCCFEFBBB60116F94E2F11236999315"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using JourneyDB_CW;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
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


namespace JourneyDB_CW {
    
    
    /// <summary>
    /// LogSign
    /// </summary>
    public partial class LogSign : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl MainTabControl;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LogInEmailTextBox;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox LogInPasswordBox;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button LogInButton;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox FirstNameTextBox;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LastNameTextBox;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EmailTextBox;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox PasswordBox;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker BirthDatePicker;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\LogSign.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SignUpButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.2.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/JourneyDB_CW;component/logsign.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\LogSign.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.2.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MainTabControl = ((System.Windows.Controls.TabControl)(target));
            return;
            case 2:
            this.LogInEmailTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 18 "..\..\..\LogSign.xaml"
            this.LogInEmailTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.EmailTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.LogInPasswordBox = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 4:
            this.LogInButton = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\..\LogSign.xaml"
            this.LogInButton.Click += new System.Windows.RoutedEventHandler(this.LogInButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.FirstNameTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.LastNameTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.EmailTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 39 "..\..\..\LogSign.xaml"
            this.EmailTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.EmailTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.PasswordBox = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 9:
            this.BirthDatePicker = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 10:
            this.SignUpButton = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\LogSign.xaml"
            this.SignUpButton.Click += new System.Windows.RoutedEventHandler(this.SignUpButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

