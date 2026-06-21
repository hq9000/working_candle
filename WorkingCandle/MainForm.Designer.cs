namespace WorkingCandle;

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
        if (disposing)
        {
            components?.Dispose();
            _notificationService?.Dispose();
            _taskbarProgressService?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        
        // Initialize UI controls
        _startButton = new Button();
        _progressBar = new ProgressBar();
        _timeLabel = new Label();
        _pauseButton = new Button();
        _resumeButton = new Button();
        _stopButton = new Button();
        _addFiveMinutesButton = new Button();
        _subtractFiveMinutesButton = new Button();
        
        SuspendLayout();
        
        // 
        // _startButton
        // 
        _startButton.Location = new Point(100, 120);
        _startButton.Name = "_startButton";
        _startButton.Size = new Size(200, 60);
        _startButton.TabIndex = 0;
        _startButton.Text = "Start 1h Timer";
        _startButton.UseVisualStyleBackColor = true;
        _startButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _startButton.Click += StartButton_Click;
        
        // 
        // _progressBar
        // 
        _progressBar.Location = new Point(25, 50);
        _progressBar.Name = "_progressBar";
        _progressBar.Size = new Size(350, 50);
        _progressBar.TabIndex = 1;
        _progressBar.Visible = false;
        _progressBar.Style = ProgressBarStyle.Continuous;
        
        // 
        // _timeLabel
        // 
        _timeLabel.AutoSize = false;
        _timeLabel.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
        _timeLabel.Location = new Point(100, 110);
        _timeLabel.Name = "_timeLabel";
        _timeLabel.Size = new Size(200, 60);
        _timeLabel.TabIndex = 2;
        _timeLabel.Text = "60:00";
        _timeLabel.TextAlign = ContentAlignment.MiddleCenter;
        _timeLabel.Visible = false;
        
        // 
        // _pauseButton
        // 
        _pauseButton.Location = new Point(40, 210);
        _pauseButton.Name = "_pauseButton";
        _pauseButton.Size = new Size(150, 45);
        _pauseButton.TabIndex = 3;
        _pauseButton.Text = "Pause";
        _pauseButton.UseVisualStyleBackColor = true;
        _pauseButton.Font = new Font("Segoe UI", 10F);
        _pauseButton.Visible = false;
        _pauseButton.Click += PauseButton_Click;
        
        // 
        // _resumeButton
        // 
        _resumeButton.Location = new Point(40, 210);
        _resumeButton.Name = "_resumeButton";
        _resumeButton.Size = new Size(150, 45);
        _resumeButton.TabIndex = 4;
        _resumeButton.Text = "Resume";
        _resumeButton.UseVisualStyleBackColor = true;
        _resumeButton.Font = new Font("Segoe UI", 10F);
        _resumeButton.Visible = false;
        _resumeButton.Click += ResumeButton_Click;
        
        // 
        // _stopButton
        // 
        // Position: Resume x (40) + Resume width (150) + gap (20) = 210
        _stopButton.Location = new Point(210, 210);
        _stopButton.Name = "_stopButton";
        _stopButton.Size = new Size(150, 45);
        _stopButton.TabIndex = 5;
        _stopButton.Text = "Stop";
        _stopButton.UseVisualStyleBackColor = true;
        _stopButton.Font = new Font("Segoe UI", 10F);
        _stopButton.Visible = false;
        _stopButton.Click += StopButton_Click;
        
        // 
        // _subtractFiveMinutesButton (- button on the left)
        // 
        _subtractFiveMinutesButton.Location = new Point(130, 260);
        _subtractFiveMinutesButton.Name = "_subtractFiveMinutesButton";
        _subtractFiveMinutesButton.Size = new Size(70, 35);
        _subtractFiveMinutesButton.TabIndex = 6;
        _subtractFiveMinutesButton.Text = "-5m";
        _subtractFiveMinutesButton.UseVisualStyleBackColor = true;
        _subtractFiveMinutesButton.Font = new Font("Segoe UI", 9F);
        _subtractFiveMinutesButton.Visible = false;
        _subtractFiveMinutesButton.Click += SubtractFiveMinutesButton_Click;
        
        // 
        // _addFiveMinutesButton (+ button on the right)
        // 
        _addFiveMinutesButton.Location = new Point(205, 260);
        _addFiveMinutesButton.Name = "_addFiveMinutesButton";
        _addFiveMinutesButton.Size = new Size(70, 35);
        _addFiveMinutesButton.TabIndex = 7;
        _addFiveMinutesButton.Text = "+5m";
        _addFiveMinutesButton.UseVisualStyleBackColor = true;
        _addFiveMinutesButton.Font = new Font("Segoe UI", 9F);
        _addFiveMinutesButton.Visible = false;
        _addFiveMinutesButton.Click += AddFiveMinutesButton_Click;
        
        // 
        // MainForm
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 300);
        Controls.Add(_startButton);
        Controls.Add(_progressBar);
        Controls.Add(_timeLabel);
        Controls.Add(_pauseButton);
        Controls.Add(_resumeButton);
        Controls.Add(_stopButton);
        Controls.Add(_addFiveMinutesButton);
        Controls.Add(_subtractFiveMinutesButton);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Working Candle";
        
        // Load icon from file
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }
        }
        catch (IOException ex)
        {
            // Silently ignore if icon file cannot be read
            System.Diagnostics.Debug.WriteLine($"Warning: Could not load icon: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Silently ignore if we don't have permission to read the icon
            System.Diagnostics.Debug.WriteLine($"Warning: Could not load icon: {ex.Message}");
        }
        
        ResumeLayout(false);
    }

    #endregion
    
    private Button _startButton;
    private ProgressBar _progressBar;
    private Label _timeLabel;
    private Button _pauseButton;
    private Button _resumeButton;
    private Button _stopButton;
    private Button _addFiveMinutesButton;
    private Button _subtractFiveMinutesButton;
}
