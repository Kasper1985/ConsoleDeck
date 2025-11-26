namespace ConsoleDeck;

internal static class NativeMethods
{
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	public static extern bool ReleaseCapture();
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
}

public partial class MainForm : Form
{
	private readonly NotifyIcon trayIcon;
	private DeckButton? enabledButton = null;

	public MainForm()
	{
		InitializeComponent();

		trayIcon = InitializeTray("ConsoleDeck");

		WindowState = FormWindowState.Minimized;
		ShowInTaskbar = false;
		
		FormClosing += MainForm_FormClosing!;
	}

	private NotifyIcon InitializeTray(string text)
	{
		var menuStrip = new ContextMenuStrip();
		menuStrip.Items.Add("Show", null, OnShowClicked!);
		menuStrip.Items.Add("Exit", null, OnExitClicked!);

		var notifyIcon = new NotifyIcon
		{
			Text = text,
			Visible = true,
			Icon = new Icon("stream_deck_actions.ico"),
			ContextMenuStrip = menuStrip
		};
		notifyIcon.DoubleClick += TrayIcon_DoubleClick!;

		return notifyIcon;
	}


	private void TrayIcon_DoubleClick(object sender, EventArgs e)
	{
		ShowMainWindow();
	}

	private void OnShowClicked(object sender, EventArgs e)
	{
		ShowMainWindow();
	}

	private void OnExitClicked(object sender, EventArgs e)
	{
		trayIcon.Visible = false;
		Application.Exit();
	}

	private void ShowMainWindow()
	{
		Show();
		WindowState = FormWindowState.Normal;
		ShowInTaskbar = true;
		BringToFront();
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		// Always minimize to tray on 'X' (UserClosing)
		if (e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			WindowState = FormWindowState.Minimized;
			ShowInTaskbar = false;
			Hide();
			
			enabledButton?.IsEnabled = false;
			enabledButton = null;
			return;
		}
		// Only hide tray icon if actually exiting from context menu
		trayIcon.Visible = false;
	}

	public void ShowKeyEvent(string text)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<string>(ShowKeyEvent), text);
			return;
		}

		if (!string.IsNullOrWhiteSpace(text))
        {
            var keys = text.Split("+").ToList();
			if (keys.Count == 3)
            {
                if (!keys.Pop("RControlKey"))
					return;

				if (!keys.Pop("RShiftKey"))
					return;

				switch (keys[0])
				{
					case "F13":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_8")?.PerformAction();
						break;
					case "F14":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_7")?.PerformAction();
						break;
					case "F15":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_6")?.PerformAction();
						break;
					case "F16":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_5")?.PerformAction();
						break;
					case "F17":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_4")?.PerformAction();
						break;
					case "F18":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_3")?.PerformAction();
						break;
					case "F19":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_2")?.PerformAction();
						break;
					case "F20":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_1")?.PerformAction();
						break;
					case "F21":
						Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "actButton_0")?.PerformAction();
						break;
				}
            }
			else if (text == "VolumeMute")
            {
                Controls.OfType<DeckRotary>().FirstOrDefault(b => b.Name == "rotaryVolume")?.PerformPress();
            }
			else if (text == "VolumeUp")
			{
				Controls.OfType<DeckRotary>().FirstOrDefault(b => b.Name == "rotaryVolume")?.PerformRotation(false);
			}
			else if (text == "VolumeDown")
			{
				Controls.OfType<DeckRotary>().FirstOrDefault(b => b.Name == "rotaryVolume")?.PerformRotation(true);
			}
			else if (text == "MediaPlayPause")
            {
                Controls.OfType<DeckButton>().FirstOrDefault(b => b.Name == "btnPlayPause")?.PerformAction();
            }
        }
	}

	public void ShowConfiguration(object? sender, EventArgs e)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<object?, EventArgs>(ShowConfiguration), sender, e);
			return;
		}

        if (sender is not DeckButton deckButton)
            return;
        if (deckButton.Name == null)
			return;
	
		deckButton.IsEnabled = true;
		if (enabledButton != null && enabledButton != deckButton)
			enabledButton.IsEnabled = false;
		enabledButton = deckButton;

		var configGroupBox = Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Name == "cfgGroupBox");
		if (configGroupBox == null)
			return;

		var nameTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtActionName");
		var typeComboBox = configGroupBox.Controls.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "cmbActionType");
		var payloadTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtPayload");
		var imageTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtImagePath");
		var descTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtDescription");

		var actions = ProcessingUnit.GetActions();
		var buttonIndex = int.Parse(deckButton.Name.Split('_')[1]);
		if (buttonIndex >= 0 && buttonIndex < actions.Count)
		{
			var action = actions[buttonIndex];

			if (action != null)
			{
				nameTextBox?.Text = action.Name;
				payloadTextBox?.Text = action.Payload;
				imageTextBox?.Text = action.ImagePath;
				descTextBox?.Text = action.Description;
				typeComboBox?.SelectedItem = action.Type.ToString();

				return;
			}
		}
		
		nameTextBox?.Text = "";
		payloadTextBox?.Text = "";
		imageTextBox?.Text = "";
		descTextBox?.Text = "";
		typeComboBox?.SelectedIndex = 0;
	}

	public void TestAction(object? sender, EventArgs e)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<object?, EventArgs>(TestAction), sender, e);
			return;
		}

		var configGroupBox = Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Name == "cfgGroupBox");
		if (configGroupBox == null)
			return;

		var payloadTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtPayload");
		var payload = payloadTextBox?.Text;

		var typeComboBox = configGroupBox.Controls.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "cmbActionType");
		var typeStr = typeComboBox?.SelectedItem?.ToString();
		if (typeStr == null)
			return;

		switch (Enum.Parse<ActionType>(typeStr))
		{
			case ActionType.Command:
				Action.ExecuteCommand(payload ?? "");
				break;
			case ActionType.WebUrl:
				Action.OpenUrl(payload ?? "");
				break;
			case ActionType.Macro:
				MessageBox.Show("Macro execution is not implemented yet.");
				break;
			case ActionType.Script:
				MessageBox.Show("Script execution is not implemented yet.");
				break;
		}
	}

	public void SaveAction(object? sender, EventArgs e)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<object?, EventArgs>(SaveAction), sender, e);
			return;
		}

		var enabledButton = Controls.OfType<DeckButton>().FirstOrDefault(b => b.IsEnabled);
		if (enabledButton == null)
			return;


		var configGroupBox = Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Name == "cfgGroupBox");
		if (configGroupBox == null)
			return;

		var nameTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtActionName");
		var typeComboBox = configGroupBox.Controls.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "cmbActionType");
		var payloadTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtPayload");
		var imageTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtImagePath");
		var descTextBox = configGroupBox.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtDescription");

		if (string.IsNullOrWhiteSpace(payloadTextBox?.Text))
			return;

		var action = new Action
		(
			nameTextBox?.Text ?? "",
			descTextBox?.Text ?? "",
			Enum.Parse<ActionType>(typeComboBox?.SelectedItem?.ToString() ?? "Command"),
			payloadTextBox?.Text ?? "",
			imageTextBox?.Text ?? ""
		);

		var buttonIndex = int.Parse(enabledButton.Name.Split('_')[1]);
		ProcessingUnit.UpdateAction(buttonIndex, action);
		ProcessingUnit.SaveConfiguration();

		enabledButton.ImagePath = action.ImagePath;
		enabledButton.Invalidate();
	}
}