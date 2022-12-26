using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfGame.Controls
{
    /// <summary>
    /// Canvas is used to place child UIElements at arbitrary positions or to draw children in multiple
    /// layers.
    /// 
    /// Child positions are computed from the Left, Top properties.  These properties do
    /// not contribute to the size of the Canvas.  To position children in a way that affects the Canvas' size,
    /// use the Margin properties.
    /// 
    /// The order that children are drawn (z-order) is determined exclusively by child order.
    /// </summary>
    public class CanvasWithMargarin : Panel
    {

        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors
        /// <summary>
        ///     Default DependencyObject constructor
        /// </summary>
        /// <remarks>
        ///     Automatic determination of current Dispatcher. Use alternative constructor
        ///     that accepts a Dispatcher for best performance.
        /// </remarks>
        public CanvasWithMargarin() : base()
        {
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Updates DesiredSize of the Canvas.  Called by parent UIElement.  This is the first pass of layout.
        /// </summary>
        /// <param name="constraint">Constraint size is an "upper limit" that Canvas should not exceed.</param>
        /// <returns>Canvas' desired size.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size childConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                child.Measure(childConstraint);
            }

            return new Size();
        }

        /// <summary>
        /// Canvas computes a position for each of its children taking into account their margin and
        /// attached Canvas properties: Top, Left.  
        /// 
        /// Canvas will also arrange each of its children.
        /// </summary>
        /// <param name="arrangeSize">Size that Canvas will assume to position children.</param>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            //Canvas arranges children at their DesiredSize.
            //This means that Margin on children is actually respected and added
            //to the size of layout partition for a child. 
            //Therefore, is Margin is 10 and Left is 20, the child's ink will start at 30.

            foreach (FrameworkElement child in InternalChildren)
            {
                if (child == null) { continue; }

                double x = 0;
                double y = 0;
                double width = child.DesiredSize.Width;
                double height = child.DesiredSize.Height;
                double right = child.Margin.Right;
                double bottom = child.Margin.Bottom;

                //Compute offset of the child:
                //If Left is specified, then Right is ignored
                //If Left is not specified, then Right is used
                //If both are not there, then 0
                if (child.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    if (!double.IsNaN(right))
                    {
                        x = arrangeSize.Width - child.DesiredSize.Width;
                    }
                }
                else if (child.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    x = (arrangeSize.Width - child.DesiredSize.Width) / 2;
                }
                if (child.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    if (!double.IsNaN(bottom))
                    {
                        y = arrangeSize.Height - child.DesiredSize.Height;
                    }
                }
                else if (child.VerticalAlignment == VerticalAlignment.Center)
                    y = (arrangeSize.Height - child.DesiredSize.Height) / 2;


                if (child.HorizontalAlignment == HorizontalAlignment.Stretch)
                    width = arrangeSize.Width - right;
                if (child.VerticalAlignment == VerticalAlignment.Stretch)
                    height = arrangeSize.Height - bottom;
                child.Arrange(new Rect(x, y, width, height)); // like grid but with canvas
            }
            return arrangeSize;
        }

        /// <summary>
        /// Override of <seealso cref="UIElement.GetLayoutClip"/>.
        /// </summary>
        /// <returns>Geometry to use as additional clip if LayoutConstrained=true</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            //Canvas only clips to bounds if ClipToBounds is set, 
            //  no automatic clipping
            if (ClipToBounds)
                return new RectangleGeometry(new Rect(RenderSize));
            else
                return null;
        }

        #endregion Protected Methods
    }
}
