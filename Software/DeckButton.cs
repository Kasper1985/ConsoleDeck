using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ConsoleDeck;

public class DeckButton : Control
{
    private int _cornerRadius = 5;
    private int _buttonSize = 100;
    private Color _baseColor = ColorTranslator.FromHtml("#232323");
    private Color _borderColor = ColorTranslator.FromHtml("#2f2f2f");
    private Color _hoverColor = ColorTranslator.FromHtml("#333333");
    private Color _pressColor = ColorTranslator.FromHtml("#237f7f");
    private Color _textColor = ColorTranslator.FromHtml("#b8b8b8");
    private Color _enabledColor = ColorTranslator.FromHtml("#2f5c2f");
    private bool _isHovered = false;
    private bool _isPressed = false;
    private bool _isEnabled = false;

    [DefaultValue(10)]
    [Category("Appearance")]
    [Description("Gets or sets the corner radius of the button")]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; Invalidate(); }
    }

    [DefaultValue(false)]
    [Category("Appearance")]
    [Description("Gets or sets whether the button is enabled")]
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; Invalidate(); }
    }

    [DefaultValue(100)]
    [Category("Appearance")]
    [Description("Gets or sets the size of the button (width and height)")]
    public int ButtonSize
    {
        get => _buttonSize;
        set 
        { 
            _buttonSize = value;
            Size = new Size(_buttonSize, _buttonSize);
            Invalidate();
        }
    }

    [DefaultValue(typeof(Color), "#232323")]
    [Category("Appearance")]
    [Description("Gets or sets the base color of the button")]
    public Color BaseColor
    {
        get => _baseColor;
        set { _baseColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#2f2f2f")]
    [Category("Appearance")]
    [Description("Gets or sets the border color of the button")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#333333")]
    [Category("Appearance")]
    [Description("Gets or sets the hover color of the button")]
    public Color HoverColor
    {
        get => _hoverColor;
        set { _hoverColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#237f7f")]
    [Category("Appearance")]
    [Description("Gets or sets the press color of the button")]
    public Color PressColor
    {
        get => _pressColor;
        set { _pressColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#b8b8b8")]
    [Category("Appearance")]
    [Description("Gets or sets the text color of the button")]
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; Invalidate(); }
    }

    [DefaultValue(typeof(Color), "#2f5c2f")]
    [Category("Appearance")]
    [Description("Gets or sets the enabled color of the button")]
    public Color EnabledColor
    {
        get => _enabledColor;
        set { _enabledColor = value; Invalidate(); }
    }

    [DefaultValue("")]
    [Category("Appearance")]
    [Description("Gets or sets the image path of the button")]
    public string? ImagePath { get; set; } = string.Empty;

    public DeckButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Font = new Font("Segoe UI", 12, FontStyle.Bold);
        Size = new Size(_buttonSize, _buttonSize);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Enable anti-aliasing and transparency
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.CompositingMode = CompositingMode.SourceOver;
        e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

        Color currentColor = BaseColor;
        if (_isHovered)
            currentColor = HoverColor;
        if (IsEnabled)
            currentColor = EnabledColor;
        if (_isPressed)
            currentColor = PressColor;

        using GraphicsPath path = new();
        using SolidBrush brush = new(currentColor);
        using Pen pen = new(BorderColor);
        path.AddRoundedRectangle(ClientRectangle.Resize(-1, -1), new Size(_cornerRadius, _cornerRadius));
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);

        if (!string.IsNullOrEmpty(Text))
        {
            SizeF textSize = e.Graphics.MeasureString(Text, Font);
            PointF textPos = new((Width - textSize.Width) / 2, (Height - textSize.Height) / 2);

            using SolidBrush textBrush = new(TextColor);
            e.Graphics.DrawString(Text, Font, textBrush, textPos);
        }

        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            var img = Image.FromFile(ImagePath);
            Point imgPos = new((Width - img.Width) / 2, (Height - img.Height) / 2);
            e.Graphics.DrawImage(img, new Rectangle(imgPos, img.Size));
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = false;
            Invalidate();
            OnClick(e);
        }
    }

    public void PerformAction()
    {
        _isPressed = true;
        Invalidate();

        System.Timers.Timer timer = new() { Interval = 200 };
        timer.Elapsed += (s, ev) =>
        {
            _isPressed = false;
            Invalidate();
            timer.Dispose();
        };
        timer.Start();
    }
}