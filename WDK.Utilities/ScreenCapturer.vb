Imports System.Drawing
Imports System
Imports System.Runtime.InteropServices

Namespace Utilities
    Class ScreenCapturer

#Region " Properties "
        Private oldRectangle As Rectangle
        Private hWndDesktop As IntPtr
        Private desktopRight As Integer
        Private desktopBottom As Integer
        Private isFormDeactivated As Boolean = False
        Private needGrab As Boolean = False
        Private control As Object
        Private _zoomRate As Integer = 1

        Public WriteOnly Property ZoomRate() As Integer
            Set(ByVal Value As Integer)
                _zoomRate = Value
            End Set
        End Property

        Public ReadOnly Property CaptureRectangle() As Rectangle
            Get
                Return oldRectangle
            End Get
        End Property


#End Region

        Public Sub New(ByVal control As Object)
            Me.control = control
            hWndDesktop = IntPtr.Zero
            desktopRight = Screen.PrimaryScreen.Bounds.Right - 1
            desktopBottom = Screen.PrimaryScreen.Bounds.Bottom - 1
        End Sub 'New


        Public Sub BeginCapture(ByVal cursorPosition As Point)
            needGrab = True
            Dim rectangle As Rectangle = GetZoomedRectangle(cursorPosition.X, cursorPosition.Y)
            DrawRectangle(rectangle)
            oldRectangle = rectangle

        End Sub 'BeginCapture


        Public Function StopCapture() As Rectangle
            If needGrab OrElse isFormDeactivated Then
                If isFormDeactivated Then
                    isFormDeactivated = False
                End If
                DrawRectangle(oldRectangle)
            End If
            needGrab = False
            Return oldRectangle

        End Function 'StopCapture


        Public Sub CopyImage(ByVal cursorPosition As Point)
            If Not needGrab Then
                Return
            End If
            Dim rectangle As Rectangle = GetZoomedRectangle(cursorPosition.X, cursorPosition.Y)
            DrawRectangle(oldRectangle)
            CopyDesktopRectangle(rectangle.Left, rectangle.Top, Nothing)
            DrawRectangle(rectangle)
            oldRectangle = rectangle

        End Sub 'CopyImage


        Public Sub PaintControl(ByVal graphics As Graphics)
            CopyDesktopRectangle(oldRectangle.Left, oldRectangle.Top, graphics)

        End Sub 'PaintControl


        '  Destkop'e ,    
        Sub DrawRectangle(ByVal rectangle As Rectangle)
            Dim hDCDesktop As IntPtr = Win32.GetDC(hWndDesktop)
            Win32.SetROP2(hDCDesktop, Win32.R2_NOT)

            Win32.MoveTo(hDCDesktop, rectangle.Left, rectangle.Top)
            Win32.LineTo(hDCDesktop, rectangle.Right, rectangle.Top)
            Win32.LineTo(hDCDesktop, rectangle.Right, rectangle.Bottom)
            Win32.LineTo(hDCDesktop, rectangle.Left, rectangle.Bottom)
            Win32.LineTo(hDCDesktop, rectangle.Left, rectangle.Top)

            Win32.SetROP2(hDCDesktop, Win32.R2_NOP)
            Win32.ReleaseDC(hWndDesktop, hDCDesktop)

        End Sub 'DrawRectangle


        '/ <summary>
        '/  Rectangle     
        '/ 
        '/ </summary>
        Function GetZoomedRectangle(ByVal x As Integer, ByVal y As Integer) As Rectangle
            Dim point As Point = control.PointToScreen(New Point(x, y))
            x = point.X
            y = point.Y

            Dim halfWidth As Integer = control.Width / ZoomRate / 2
            Dim halfHeight As Integer = control.Height / ZoomRate / 2

            Dim left As Integer = x - halfWidth
            Dim top As Integer = y - halfHeight

            If left < 0 Then
                left = 0
            End If
            If top < 0 Then
                top = 0
            End If
            Dim right As Integer = left + control.Width / ZoomRate - 1
            Dim bottom As Integer = top + control.Height / ZoomRate - 1

            If right > desktopRight Then
                right = desktopRight
                left = right - control.Width / ZoomRate + 1
            End If

            If bottom > desktopBottom Then
                bottom = desktopBottom
                top = bottom - control.Height / ZoomRate + 1
            End If

            Return Rectangle.FromLTRB(left, top, right, bottom)

        End Function 'GetZoomedRectangle


        '/ <summary>
        '/     Desktop'a  Panel
        '/ </summary>
        '/ <param name="left">X     Desktop'a</param>
        '/ <param name="top">Y     Desktop'a</param>
        '/ <param name="createDesktopDC">   HDC Desktop'a?</param>
        Sub CopyDesktopRectangle(ByVal left As Integer, ByVal top As Integer, ByVal graphics As Graphics)
            If graphics Is Nothing Then
                graphics = control.CreateGraphics()
            End If
            Dim hDCPanel As IntPtr = graphics.GetHdc()
            Dim hDCDesktop As IntPtr = Win32.GetDC(hWndDesktop)

            Win32.StretchBlt(hDCPanel, 0, 0, control.Width, control.Height, hDCDesktop, left, top, control.Width / ZoomRate, control.Height / ZoomRate, Win32.SRCCOPY)

            graphics.ReleaseHdc(hDCPanel)
            Win32.ReleaseDC(hWndDesktop, hDCDesktop)

        End Sub 'CopyDesktopRectangle


        Public WriteOnly Property DesktopDimension() As Point
            Set(ByVal Value As Point)
                desktopRight = Value.X - 1
                desktopBottom = Value.Y - 1
            End Set
        End Property
    End Class 'ScreenCapturer

End Namespace