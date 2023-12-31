﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFTest.UI.Menu
{
    /// <summary>
    /// MenuPageChapter1.xaml 的交互逻辑
    /// </summary>
    public partial class MenuPageChapter1 : ChildPage
    {
        public MenuPageChapter1()
        {
            m_objectCur = null;

            InitializeComponent();
        }

        private void sy1_Click(object sender, RoutedEventArgs e)
        {
            FireNextEvent("chapter1_sy1");
        }

        private void sy2_Click(object sender, RoutedEventArgs e)
        {
            FireNextEvent("chapter1_sy2");
        }

        private void ChildPage_Loaded(object sender, RoutedEventArgs e)
        {
            FireNextEvent("chapter1_sy1");
        }

        private void btnLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (m_objectCur != null)
            {
                ((Button)m_objectCur).Foreground = (SolidColorBrush)this.Resources["submenu_foreground"];
            }
            ((Button)sender).Foreground = (SolidColorBrush)this.Resources["submenu_foreground_hot"];

            m_objectCur = (Button)sender;
        }
    }
}
