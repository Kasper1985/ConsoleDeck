using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace ConsoleDeck;

partial class MainForm
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null)) components.Dispose();
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
		{
			GraphicsPath path = new();
			int diameter = radius * 2;
			// Top left arc
			path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
			// Top right arc
			path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
			// Bottom right arc
			path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
			// Bottom left arc
			path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
			path.CloseFigure();
			return path;
		}

		void SetRoundedRegion(int radius)
        {
			using var path = GetRoundedRectPath(this.ClientRectangle, radius);
			this.Region = new Region(path);
		}


		this.components = new System.ComponentModel.Container();
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ClientSize = new Size(800, 420);
		this.Text = "Stream Deck Actions";
		var cornerRadius = 8;

		// Apply rounded corners and border (20px radius, border #2f2f2f)
		this.Load += (s, e) => { SetRoundedRegion(cornerRadius); };
		this.SizeChanged += (s, e) =>
		{
			SetRoundedRegion(cornerRadius);
			this.Invalidate();
		};
		this.Paint += (s, e) =>
		{
			using Pen pen = new(ColorTranslator.FromHtml("#2f2f2f"), 2);
			using var path = GetRoundedRectPath(this.ClientRectangle, cornerRadius);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.DrawPath(pen, path);
		};
		this.BackColor = ColorTranslator.FromHtml("#141414");
		this.ForeColor = ColorTranslator.FromHtml("#b8b8b8");
		this.Font = SystemFonts.DefaultFont;
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		try
		{
			this.Icon = new Icon("stream_deck_actions.ico");
		}
		catch { }

		// Custom title bar
		var titleBar = new System.Windows.Forms.Panel();
		titleBar.Name = "panelTitleBar";
		titleBar.Dock = System.Windows.Forms.DockStyle.Top;
		titleBar.Height = 30;
		titleBar.BackColor = ColorTranslator.FromHtml("#1f1f1f");
		titleBar.Padding = new System.Windows.Forms.Padding(1, 1, 1, 0); // 1px for border
		titleBar.BringToFront();
		titleBar.Paint += (s, e) =>
		{
			// Draw border on top of the title bar as well
			using Pen pen = new(ColorTranslator.FromHtml("#2f2f2f"), 2);
			using var path = GetRoundedRectPath(this.ClientRectangle, cornerRadius);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.DrawPath(pen, path);
		};

		var titleLabel = new System.Windows.Forms.Label();
		titleLabel.Text = this.Text;
		titleLabel.ForeColor = ColorTranslator.FromHtml("#b8b8b8");
		titleLabel.Font = new Font(this.Font.FontFamily, 9, System.Drawing.FontStyle.Bold);
		titleLabel.AutoSize = true;
		titleLabel.Location = new System.Drawing.Point(12, 6);
		titleBar.Controls.Add(titleLabel);
		this.Controls.Add(titleBar);

		// Add close button
		var closeButton = new CloseButton();
		closeButton.Name = "closeButton";
		closeButton.Size = new Size(40, 30);
		closeButton.Location = new Point(this.ClientSize.Width - 40, 0);
		closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		closeButton.Click += (s, e) => this.Close();
		titleBar.Controls.Add(closeButton);

		// Enable dragging the window by the title bar
		titleBar.MouseDown += (s, e) =>
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				NativeMethods.ReleaseCapture();
				NativeMethods.SendMessage(this.Handle, 0xA1, 0x2, 0);
			}
		};

		// === Custom Controls Layout ===
		// 3x3 grid of action buttons
		int buttonSize = 100;
		int spacing = 15;
		int startX = 25;
		int startY = 60;

		var actions = ProcessingUnit.GetActions();
		for (int row = 0; row < 3; row++)
			for( int col = 0; col < 3; col++)
			{
				var actButton = new DeckButton();
				var index = row * 3 + col;
				actButton.Name = $"actButton_{index}";
				if (index < actions.Count)
					actButton.ImagePath = actions[index]?.ImagePath;
				actButton.Location = new System.Drawing.Point(
					startX + col * (buttonSize + spacing),
					startY + row * (buttonSize + spacing)
				);
				actButton.Click += ShowConfiguration;
				this.Controls.Add(actButton);
			}

		// Play / Pause button
		var playButton = new DeckButton();
		playButton.Name = "btnPlayPause";
		playButton.Text = "";
		playButton.ImagePath = "button_images\\play_pause.png";
		playButton.Size = new System.Drawing.Size((int)(buttonSize * 1.4), buttonSize);
		playButton.Location = new System.Drawing.Point(
			startX + 3 * (buttonSize + spacing),
			startY + 2 * (buttonSize + spacing)
		);
		this.Controls.Add(playButton);

		// Rotary encoder control
		var rotaryControl = new DeckRotary();
		rotaryControl.Name = "rotaryVolume";
		rotaryControl.Text = "";
		rotaryControl.ImagePath = "button_images\\volume.png";
		rotaryControl.Size = new System.Drawing.Size((int)(buttonSize * 1.4), (int)(buttonSize * 1.4));
		rotaryControl.Location = new System.Drawing.Point(
			startX + 3 * (buttonSize + spacing) + (int)((playButton.Width - rotaryControl.Width) / 2),
			startY + (int)((2 * buttonSize + spacing - rotaryControl.Height) / 2)
		);
		this.Controls.Add(rotaryControl);
	
		// Configuration secion
		var configGroupBox = new GroupBox();
		configGroupBox.Text = " Configuration ";
		configGroupBox.Name = "cfgGroupBox";
		configGroupBox.ForeColor = ColorTranslator.FromHtml("#b8b8b8");
		configGroupBox.Font = new Font(this.Font.FontFamily, 9, System.Drawing.FontStyle.Bold);
		// calculate size and position
		var size = new Size(
			this.ClientSize.Width - (2 *startX + 3 * (buttonSize + spacing) + (int)(buttonSize * 1.4) + spacing),
			this.ClientSize.Height - (int)(1.5 * startY)
		);
		var location = new Point(
			startX + 3 * (buttonSize + spacing) + (int)(buttonSize * 1.4) + spacing,
			startY
		);
		configGroupBox.Size = size;
		configGroupBox.Location = location;
		this.Controls.Add(configGroupBox);

		// Configuration controls
		var nameLabel = new System.Windows.Forms.Label();
		nameLabel.Text = "Action Name:";
		nameLabel.AutoSize = true;
		nameLabel.Location = new Point(15, 20);
		configGroupBox.Controls.Add(nameLabel);
		var nameTextBox = new System.Windows.Forms.TextBox();
		nameTextBox.Name = "txtActionName";
		nameTextBox.Size = new Size(size.Width - 30, 23);
		nameTextBox.Location = new Point(15, 40);
		configGroupBox.Controls.Add(nameTextBox);

		var typeLabel = new System.Windows.Forms.Label();
		typeLabel.Text = "Action Type:";
		typeLabel.AutoSize = true;
		typeLabel.Location = new Point(15, 65);
		configGroupBox.Controls.Add(typeLabel);
		var typeComboBox = new System.Windows.Forms.ComboBox();
		typeComboBox.Name = "cmbActionType";
		typeComboBox.Size = new Size(size.Width - 30, 23);
		typeComboBox.Location = new Point(15, 85);
		typeComboBox.Items.AddRange(Enum.GetNames(typeof(ActionType)));
		typeComboBox.SelectedIndex = 0;
		configGroupBox.Controls.Add(typeComboBox);

		var payloadLabel = new System.Windows.Forms.Label();
		payloadLabel.Text = "Payload:";
		payloadLabel.AutoSize = true;
		payloadLabel.Location = new Point(15, 112);
		configGroupBox.Controls.Add(payloadLabel);
		var payloadTextBox = new System.Windows.Forms.TextBox();
		payloadTextBox.Name = "txtPayload";
		payloadTextBox.Size = new Size(size.Width - 30, 23);
		payloadTextBox.Location = new Point(15, 132);
		configGroupBox.Controls.Add(payloadTextBox);

		var imageLabel = new System.Windows.Forms.Label();
		imageLabel.Text = "Image Path:";
		imageLabel.AutoSize = true;
		imageLabel.Location = new Point(15, 157);
		configGroupBox.Controls.Add(imageLabel);
		var imageTextBox = new System.Windows.Forms.TextBox();
		imageTextBox.Name = "txtImagePath";
		imageTextBox.Size = new Size(size.Width - 30, 23);
		imageTextBox.Location = new Point(15, 177);
		configGroupBox.Controls.Add(imageTextBox);

		var descLabel = new System.Windows.Forms.Label();
		descLabel.Text = "Description:";
		descLabel.AutoSize = true;
		descLabel.Location = new Point(15, 202);
		configGroupBox.Controls.Add(descLabel);
		var descTextBox = new System.Windows.Forms.TextBox();
		descTextBox.Name = "txtDescription";
		descTextBox.Size = new Size(size.Width - 30, 23);
		descTextBox.Location = new Point(15, 222);
		configGroupBox.Controls.Add(descTextBox);

		var testButton = new System.Windows.Forms.Button();
		testButton.Name = "btnTestAction";
		testButton.Text = "";
		testButton.BackgroundImage = Image.FromFile("button_images\\test.png");
		testButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		testButton.Cursor = System.Windows.Forms.Cursors.Hand;
		testButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		testButton.FlatAppearance.BorderSize = 0;
		testButton.Size = new Size(30, 30);
		testButton.Location = new Point(size.Width - 90, size.Height - 45);
		testButton.Click += TestAction;
		configGroupBox.Controls.Add(testButton);

		var saveButton = new System.Windows.Forms.Button();
		saveButton.Name = "btnSaveAction";
		saveButton.Text = "";
		saveButton.BackgroundImage = Image.FromFile("button_images\\save.png");
		saveButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		saveButton.Cursor = System.Windows.Forms.Cursors.Hand;
		saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		saveButton.FlatAppearance.BorderSize = 0;
		saveButton.Size = new Size(30, 30);
		saveButton.Location = new Point(size.Width - 45, size.Height - 45);
		saveButton.Click += SaveAction;
		configGroupBox.Controls.Add(saveButton);
	}

	#endregion
}