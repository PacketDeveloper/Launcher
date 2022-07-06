using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PacketClientInjector
{
    public class MainForm : Form
	{
		private Color hoverTextColor = Color.FromArgb(124, 124, 124);

		private Color downTextColor = Color.FromArgb(200, 200, 200);

		private Color hoverBackColor = Color.FromArgb(66, 66, 66);

		private Color downBackColor = Color.FromArgb(88, 88, 88);

		private Color normalBackColor = Color.FromArgb(26, 26, 26);

		private const int PROCESS_CREATE_THREAD = 2;

		private const int PROCESS_QUERY_INFORMATION = 1024;

		private const int PROCESS_VM_OPERATION = 8;

		private const int PROCESS_VM_WRITE = 32;

		private const int PROCESS_VM_READ = 16;

		private const uint MEM_COMMIT = 4096;

		private const uint MEM_RESERVE = 8192;

		private const uint PAGE_READWRITE = 4;

		private string DllPath;

		private static bool alreadyAttemptedInject;

		private bool drag;

		private Point start_point = new Point(0, 0);

		private Color activeBorderColor = Color.FromArgb(255, 255, 255);

		private Color inactiveBorderColor = Color.FromArgb(64, 64, 64);

		private Color activeTextColor = Color.FromArgb(255, 255, 255);

		private Color inactiveTextColor = Color.FromArgb(177, 177, 177);

		private DateTime titleClickTime = DateTime.MinValue;

		private Point titleClickPosition = Point.Empty;

		private IContainer components;

		private Label MinimizeLabel;

		private ToolTip DecorationToolTip;

		private Label CloseLabel;

		private Label TitleLabel;

		private Panel RightBorderPanel;

		private Panel LeftBorderPanel;

		private Panel BottomBorderPanel;

		private Panel TopBorderPanel;

		private TabControl TabControl;

		private TabPage Home;

		private TabPage Dev;

		private Label HomePageTab;

		private Label DevInjectPageTab;

		private Label HelpPageTab;

		private PictureBox pictureBox1;

		private TabPage Help;

		private Label InjectButton;

		private Label Infolabel;

		private Panel ProgressBar;

		private PictureBox pictureBox2;

		private Label InjectDllButton;

		private PictureBox pictureBox3;

		private Label DevInfoText;

		private Label LocateDllButton;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private Panel uselessthingjustneededfortheinjectfunction;
        private Button Devbutton;

        public Color ActiveBorderColor
		{
			get
			{
				return this.activeBorderColor;
			}
			set
			{
				this.activeBorderColor = value;
			}
		}

		public Color ActiveTextColor
		{
			get
			{
				return this.activeTextColor;
			}
			set
			{
				this.activeTextColor = value;
			}
		}

		public Color DownBackColor
		{
			get
			{
				return this.downBackColor;
			}
			set
			{
				this.downBackColor = value;
			}
		}

		public Color DownTextColor
		{
			get
			{
				return this.downTextColor;
			}
			set
			{
				this.downTextColor = value;
			}
		}

		public Color HoverBackColor
		{
			get
			{
				return this.hoverBackColor;
			}
			set
			{
				this.hoverBackColor = value;
			}
		}

		public Color HoverTextColor
		{
			get
			{
				return this.hoverTextColor;
			}
			set
			{
				this.hoverTextColor = value;
			}
		}

		public Color InactiveBorderColor
		{
			get
			{
				return this.inactiveBorderColor;
			}
			set
			{
				this.inactiveBorderColor = value;
			}
		}

		public Color InactiveTextColor
		{
			get
			{
				return this.inactiveTextColor;
			}
			set
			{
				this.inactiveTextColor = value;
			}
		}

		public Color NormalBackColor
		{
			get
			{
				return this.normalBackColor;
			}
			set
			{
				this.normalBackColor = value;
			}
		}

		public MainForm()
		{
			int i;
			this.InitializeComponent();
			base.Activated += new EventHandler(this.MainForm_Activated);
			base.Deactivate += new EventHandler(this.MainForm_Deactivate);
			Label[] homePageTab = new Label[] { this.HomePageTab, this.DevInjectPageTab, this.HelpPageTab, this.MinimizeLabel, this.CloseLabel };
			for (i = 0; i < (int)homePageTab.Length; i++)
			{
				Label label = homePageTab[i];
				label.MouseEnter += new EventHandler((object s, EventArgs e) => this.SetLabelColors((Control)s, MainForm.MouseState.Hover));
				label.MouseLeave += new EventHandler((object s, EventArgs e) => this.SetLabelColors((Control)s, MainForm.MouseState.Normal));
				label.MouseDown += new MouseEventHandler((object s, MouseEventArgs e) => this.SetLabelColors((Control)s, MainForm.MouseState.Down));
			}
			homePageTab = new Label[] { this.InjectButton, this.LocateDllButton, this.InjectDllButton };
			for (i = 0; i < (int)homePageTab.Length; i++)
			{
				Label label1 = homePageTab[i];
				label1.MouseEnter += new EventHandler((object s, EventArgs e) => this.SetLabelBackgroundOnly((Control)s, MainForm.MouseState.Hover));
				label1.MouseLeave += new EventHandler((object s, EventArgs e) => this.SetLabelBackgroundOnly((Control)s, MainForm.MouseState.Normal));
				label1.MouseDown += new MouseEventHandler((object s, MouseEventArgs e) => this.SetLabelBackgroundOnly((Control)s, MainForm.MouseState.Down));
			}
			this.TitleLabel.Text = this.Text;
			base.TextChanged += new EventHandler((object s, EventArgs e) => this.TitleLabel.Text = this.Text);
			Font font = new Font("Marlett", 8.5f);
			this.MinimizeLabel.Font = font;
			this.CloseLabel.Font = font;
			Font font1 = new Font("Minecraft", 20.25f);
			this.InjectButton.Font = font1;
			this.CloseLabel.MouseClick += new MouseEventHandler((object s, MouseEventArgs e) => this.Close(e));
			this.InitialValues();
		}

		public static void applyAppPackages(string DLLPath)
		{
			FileInfo fileInfo = new FileInfo(DLLPath);
			FileSecurity accessControl = fileInfo.GetAccessControl();
			accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			fileInfo.SetAccessControl(accessControl);
		}

		private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			base.BeginInvoke(new MethodInvoker(() => MainForm.InjectDLL(string.Concat(Path.GetTempPath(), "/PacketClient.dll"), this.Infolabel, this.ProgressBar, this)));
		}

		private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			base.BeginInvoke(new MethodInvoker(() => {
				long bytesReceived = e.BytesReceived;
				double num = double.Parse(bytesReceived.ToString());
				bytesReceived = e.TotalBytesToReceive;
				double num1 = double.Parse(bytesReceived.ToString());
				double num2 = num / num1 * 100;
				Label infolabel = this.Infolabel;
				string[] str = new string[] { "Downloading ", null, null, null, null, null, null };
				int num3 = Convert.ToInt32(num2);
				str[1] = num3.ToString();
				str[2] = "%  -  ";
				num3 = Convert.ToInt32(num / 1024 / 1024);
				str[3] = num3.ToString();
				str[4] = " / ";
				num3 = Convert.ToInt32(num1 / 1024 / 1024);
				str[5] = num3.ToString();
				str[6] = " mb";
				infolabel.Text = string.Concat(str);
				double num4 = Math.Round(2.7 * num2, 0);
				this.ProgressBar.Width = int.Parse(Math.Truncate(num4).ToString());
			}));
		}

		private void Close(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				base.Close();
			}
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		private void DevInjectPageTab_Click(object sender, EventArgs e)
		{
			this.TabControl.SelectedIndex = 1;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		private void HelpPageTab_Click(object sender, EventArgs e)
		{
			this.TabControl.SelectedIndex = 2;
		}

		private void HomePageTab_Click(object sender, EventArgs e)
		{
			this.TabControl.SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
            this.components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            this.MinimizeLabel = new Label();
            this.CloseLabel = new Label();
            this.TitleLabel = new Label();
            this.RightBorderPanel = new Panel();
            this.LeftBorderPanel = new Panel();
            this.BottomBorderPanel = new Panel();
            this.TopBorderPanel = new Panel();
            this.DecorationToolTip = new ToolTip(this.components);
            this.TabControl = new TabControl();
            this.Home = new TabPage();
            this.pictureBox4 = new PictureBox();
            this.pictureBox2 = new PictureBox();
            this.ProgressBar = new Panel();
            this.Infolabel = new Label();
            this.InjectButton = new Label();
            this.Dev = new TabPage();
            this.pictureBox5 = new PictureBox();
            this.uselessthingjustneededfortheinjectfunction = new Panel();
            this.InjectDllButton = new Label();
            this.pictureBox3 = new PictureBox();
            this.DevInfoText = new Label();
            this.LocateDllButton = new Label();
            this.Help = new TabPage();
            this.HomePageTab = new Label();
            this.DevInjectPageTab = new Label();
            this.HelpPageTab = new Label();
            this.pictureBox1 = new PictureBox();
            this.TabControl.SuspendLayout();
            this.Home.SuspendLayout();
            ((ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.Dev.SuspendLayout();
            ((ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // MinimizeLabel
            // 
            this.MinimizeLabel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.MinimizeLabel.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.MinimizeLabel.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeLabel.Location = new Point(683, 1);
            this.MinimizeLabel.Name = "MinimizeLabel";
            this.MinimizeLabel.Size = new Size(23, 23);
            this.MinimizeLabel.TabIndex = 20;
            this.MinimizeLabel.Text = "0";
            this.MinimizeLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.DecorationToolTip.SetToolTip(this.MinimizeLabel, "Minimize");
            this.MinimizeLabel.Click += new EventHandler(this.MinimizeLabel_Click);
            // 
            // CloseLabel
            // 
            this.CloseLabel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.CloseLabel.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.CloseLabel.Font = new Font("Marlett", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.CloseLabel.Location = new Point(709, 1);
            this.CloseLabel.Name = "CloseLabel";
            this.CloseLabel.Size = new Size(23, 23);
            this.CloseLabel.TabIndex = 22;
            this.CloseLabel.Text = "r";
            this.CloseLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.DecorationToolTip.SetToolTip(this.CloseLabel, "Close");
            // 
            // TitleLabel
            // 
            this.TitleLabel.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.TitleLabel.Dock = DockStyle.Top;
            this.TitleLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.TitleLabel.ForeColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.TitleLabel.Location = new Point(0, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new Size(734, 24);
            this.TitleLabel.TabIndex = 23;
            this.TitleLabel.Text = "Title";
            this.TitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.TitleLabel.Click += new EventHandler(this.TitleLabel_Click);
            this.TitleLabel.MouseDown += new MouseEventHandler(this.TitleLabel_MouseDown);
            this.TitleLabel.MouseMove += new MouseEventHandler(this.TitleLabel_MouseMove);
            this.TitleLabel.MouseUp += new MouseEventHandler(this.TitleLabel_MouseUp);
            // 
            // RightBorderPanel
            // 
            this.RightBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Right)));
            this.RightBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.RightBorderPanel.Cursor = Cursors.SizeWE;
            this.RightBorderPanel.Location = new Point(733, 1);
            this.RightBorderPanel.Name = "RightBorderPanel";
            this.RightBorderPanel.Size = new Size(1, 409);
            this.RightBorderPanel.TabIndex = 19;
            // 
            // LeftBorderPanel
            // 
            this.LeftBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Left)));
            this.LeftBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.LeftBorderPanel.Cursor = Cursors.SizeWE;
            this.LeftBorderPanel.Location = new Point(0, 1);
            this.LeftBorderPanel.Name = "LeftBorderPanel";
            this.LeftBorderPanel.Size = new Size(1, 409);
            this.LeftBorderPanel.TabIndex = 18;
            // 
            // BottomBorderPanel
            // 
            this.BottomBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.BottomBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.BottomBorderPanel.Cursor = Cursors.SizeNS;
            this.BottomBorderPanel.Location = new Point(-5, 410);
            this.BottomBorderPanel.Name = "BottomBorderPanel";
            this.BottomBorderPanel.Size = new Size(742, 1);
            this.BottomBorderPanel.TabIndex = 17;
            // 
            // TopBorderPanel
            // 
            this.TopBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.TopBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.TopBorderPanel.Cursor = Cursors.SizeNS;
            this.TopBorderPanel.Location = new Point(-2, 0);
            this.TopBorderPanel.Name = "TopBorderPanel";
            this.TopBorderPanel.Size = new Size(737, 1);
            this.TopBorderPanel.TabIndex = 16;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.Home);
            this.TabControl.Controls.Add(this.Dev);
            this.TabControl.Controls.Add(this.Help);
            this.TabControl.Location = new Point(-7, -1);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new Size(749, 416);
            this.TabControl.TabIndex = 25;
            this.TabControl.SelectedIndexChanged += new EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // Home
            // 
            this.Home.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.Home.Controls.Add(this.pictureBox4);
            this.Home.Controls.Add(this.pictureBox2);
            this.Home.Controls.Add(this.ProgressBar);
            this.Home.Controls.Add(this.Infolabel);
            this.Home.Controls.Add(this.InjectButton);
            this.Home.Location = new Point(4, 22);
            this.Home.Name = "Home";
            this.Home.Padding = new Padding(3);
            this.Home.Size = new Size(741, 390);
            this.Home.TabIndex = 0;
            this.Home.Text = "Home";
            this.Home.Click += new EventHandler(this.Home_Click);
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.pictureBox4.BackgroundImage = ((Image)(resources.GetObject("pictureBox4.BackgroundImage")));
            this.pictureBox4.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox4.Location = new Point(690, 357);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new Size(47, 30);
            this.pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 30;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new EventHandler(this.pictureBox4_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.pictureBox2.BackgroundImage = ((Image)(resources.GetObject("pictureBox2.BackgroundImage")));
            this.pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox2.Location = new Point(49, 50);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new Size(634, 100);
            this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 30;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new EventHandler(this.pictureBox2_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.BackColor = Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.ProgressBar.Location = new Point(233, 186);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new Size(270, 7);
            this.ProgressBar.TabIndex = 33;
            // 
            // Infolabel
            // 
            this.Infolabel.AutoSize = true;
            this.Infolabel.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.Infolabel.ForeColor = Color.White;
            this.Infolabel.Location = new Point(6, 369);
            this.Infolabel.Name = "Infolabel";
            this.Infolabel.Size = new Size(110, 24);
            this.Infolabel.TabIndex = 32;
            this.Infolabel.Text = "Not Injected";
            // 
            // InjectButton
            // 
            this.InjectButton.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.InjectButton.BorderStyle = BorderStyle.FixedSingle;
            this.InjectButton.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.InjectButton.ForeColor = Color.Gray;
            this.InjectButton.Location = new Point(233, 198);
            this.InjectButton.Name = "InjectButton";
            this.InjectButton.Size = new Size(270, 80);
            this.InjectButton.TabIndex = 30;
            this.InjectButton.Text = "Inject";
            this.InjectButton.TextAlign = ContentAlignment.MiddleCenter;
            this.InjectButton.Click += new EventHandler(this.InjectButton_Click);
            // 
            // Dev
            // 
            this.Dev.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.Dev.Controls.Add(this.pictureBox5);
            this.Dev.Controls.Add(this.uselessthingjustneededfortheinjectfunction);
            this.Dev.Controls.Add(this.InjectDllButton);
            this.Dev.Controls.Add(this.pictureBox3);
            this.Dev.Controls.Add(this.DevInfoText);
            this.Dev.Controls.Add(this.LocateDllButton);
            this.Dev.Location = new Point(4, 22);
            this.Dev.Name = "Dev";
            this.Dev.Padding = new Padding(3);
            this.Dev.Size = new Size(741, 390);
            this.Dev.TabIndex = 1;
            this.Dev.Text = "Dev";
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.pictureBox5.BackgroundImage = ((Image)(resources.GetObject("pictureBox5.BackgroundImage")));
            this.pictureBox5.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox5.Location = new Point(691, 357);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new Size(45, 30);
            this.pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox5.TabIndex = 30;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Click += new EventHandler(this.pictureBox5_Click);
            // 
            // uselessthingjustneededfortheinjectfunction
            // 
            this.uselessthingjustneededfortheinjectfunction.Location = new Point(999, 999);
            this.uselessthingjustneededfortheinjectfunction.Name = "uselessthingjustneededfortheinjectfunction";
            this.uselessthingjustneededfortheinjectfunction.Size = new Size(0, 0);
            this.uselessthingjustneededfortheinjectfunction.TabIndex = 39;
            // 
            // InjectDllButton
            // 
            this.InjectDllButton.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.InjectDllButton.BorderStyle = BorderStyle.FixedSingle;
            this.InjectDllButton.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.InjectDllButton.ForeColor = Color.White;
            this.InjectDllButton.Location = new Point(372, 198);
            this.InjectDllButton.Name = "InjectDllButton";
            this.InjectDllButton.Size = new Size(131, 80);
            this.InjectDllButton.TabIndex = 38;
            this.InjectDllButton.Text = "Inject";
            this.InjectDllButton.TextAlign = ContentAlignment.MiddleCenter;
            this.InjectDllButton.Click += new EventHandler(this.InjectDllButton_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.pictureBox3.BackgroundImage = ((Image)(resources.GetObject("pictureBox3.BackgroundImage")));
            this.pictureBox3.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox3.Location = new Point(50, 80);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new Size(634, 100);
            this.pictureBox3.TabIndex = 34;
            this.pictureBox3.TabStop = false;
            // 
            // DevInfoText
            // 
            this.DevInfoText.AutoSize = true;
            this.DevInfoText.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.DevInfoText.ForeColor = Color.White;
            this.DevInfoText.Location = new Point(6, 369);
            this.DevInfoText.Name = "DevInfoText";
            this.DevInfoText.Size = new Size(110, 24);
            this.DevInfoText.TabIndex = 36;
            this.DevInfoText.Text = "Not Injected";
            // 
            // LocateDllButton
            // 
            this.LocateDllButton.BackColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.LocateDllButton.BorderStyle = BorderStyle.FixedSingle;
            this.LocateDllButton.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.LocateDllButton.ForeColor = Color.White;
            this.LocateDllButton.Location = new Point(233, 198);
            this.LocateDllButton.Name = "LocateDllButton";
            this.LocateDllButton.Size = new Size(132, 80);
            this.LocateDllButton.TabIndex = 35;
            this.LocateDllButton.Text = "Locate";
            this.LocateDllButton.TextAlign = ContentAlignment.MiddleCenter;
            this.LocateDllButton.Click += new EventHandler(this.LocateDllButton_Click);
            // 
            // Help
            // 
            this.Help.BackColor = Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.Help.Location = new Point(4, 22);
            this.Help.Name = "Help";
            this.Help.Padding = new Padding(3);
            this.Help.Size = new Size(741, 390);
            this.Help.TabIndex = 2;
            this.Help.Text = "?";
            // 
            // HomePageTab
            // 
            this.HomePageTab.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.HomePageTab.ForeColor = Color.White;
            this.HomePageTab.Location = new Point(26, 1);
            this.HomePageTab.Name = "HomePageTab";
            this.HomePageTab.Size = new Size(65, 23);
            this.HomePageTab.TabIndex = 26;
            this.HomePageTab.Text = "Home";
            this.HomePageTab.TextAlign = ContentAlignment.MiddleCenter;
            this.HomePageTab.Click += new EventHandler(this.HomePageTab_Click);
            // 
            // DevInjectPageTab
            // 
            this.DevInjectPageTab.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.DevInjectPageTab.ForeColor = Color.White;
            this.DevInjectPageTab.Location = new Point(94, 1);
            this.DevInjectPageTab.Name = "DevInjectPageTab";
            this.DevInjectPageTab.Size = new Size(75, 23);
            this.DevInjectPageTab.TabIndex = 27;
            this.DevInjectPageTab.Text = "Development";
            this.DevInjectPageTab.TextAlign = ContentAlignment.MiddleCenter;
            this.DevInjectPageTab.Click += new EventHandler(this.DevInjectPageTab_Click);
            // 
            // HelpPageTab
            // 
            this.HelpPageTab.BackColor = Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
            this.HelpPageTab.ForeColor = Color.White;
            this.HelpPageTab.Location = new Point(172, 1);
            this.HelpPageTab.Name = "HelpPageTab";
            this.HelpPageTab.Size = new Size(70, 23);
            this.HelpPageTab.TabIndex = 28;
            this.HelpPageTab.Text = "?";
            this.HelpPageTab.TextAlign = ContentAlignment.MiddleCenter;
            this.HelpPageTab.Click += new EventHandler(this.HelpPageTab_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.pictureBox1.BackgroundImage = ((Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            this.pictureBox1.Location = new Point(1, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(24, 23);
            this.pictureBox1.TabIndex = 29;
            this.pictureBox1.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.ClientSize = new Size(734, 411);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.DevInjectPageTab);
            this.Controls.Add(this.HomePageTab);
            this.Controls.Add(this.MinimizeLabel);
            this.Controls.Add(this.CloseLabel);
            this.Controls.Add(this.RightBorderPanel);
            this.Controls.Add(this.LeftBorderPanel);
            this.Controls.Add(this.BottomBorderPanel);
            this.Controls.Add(this.TopBorderPanel);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.HelpPageTab);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Packet Injector";
            this.Load += new EventHandler(this.MainForm_Load);
            this.TabControl.ResumeLayout(false);
            this.Home.ResumeLayout(false);
            this.Home.PerformLayout();
            ((ISupportInitialize)(this.pictureBox4)).EndInit();
            ((ISupportInitialize)(this.pictureBox2)).EndInit();
            this.Dev.ResumeLayout(false);
            this.Dev.PerformLayout();
            ((ISupportInitialize)(this.pictureBox5)).EndInit();
            ((ISupportInitialize)(this.pictureBox3)).EndInit();
            ((ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}

        private void InitialValues()
		{
			this.ProgressBar.Width = 0;
		}

		private void InjectButton_Click(object sender, EventArgs e)
		{
			this.InjectButton.BackColor = Color.FromArgb(44, 44, 44);
			if (!MainForm.InternetCheck())
			{
				this.ProgressBar.Width = 0;
				this.Infolabel.Text = "Injection Failed. No Internet";
				this.Refresh();
				MessageBox.Show("     Cannot Connect To Server\n       Check Your Internet Connection");
				return;
			}
			if (Process.GetProcessesByName("Minecraft.Windows").Length == 0)
			{
				this.ProgressBar.Width = 0;
				this.Infolabel.Text = "Injection Failed";
				this.Refresh();
				MessageBox.Show("Open Minecraft First");
				return;
			}
			string str = "Packet Client Launcher";
			WebClient webClient = new WebClient();
			webClient.Headers.Add(HttpRequestHeader.UserAgent, str);
			webClient.Proxy = null;
			string str1 = "https://github.com/PacketDeveloper/PacketDeveloper/raw/main/PacketClient.dll";
			string str2 = string.Concat(Path.GetTempPath(), "/PacketClient.dll");
			this.startDownload(str1, str2);
		}

		public static void InjectDLL(string DownloadedDllFilePath, Control label, Control panel, Control Form1)
		{
            UIntPtr uIntPtr;
            Form1.Refresh();
            int OpenMC;
			OpenMC = 0;
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0)
			{
				if (!MainForm.alreadyAttemptedInject)
				{
					MainForm.alreadyAttemptedInject = true;
					panel.Width = 0;
					label.Text = "Injection Failed";
					Form1.Refresh();
					MessageBox.Show("Open Minecraft First");
                    OpenMC = 1;
                    return;
				}
                string error = "Error:\n";
				Console.WriteLine("LOGGER:\nCalled the person an Idiot");
				panel.Width = 0;
				label.Text = "Not Injected";
				Form1.Refresh();
				MessageBox.Show("Open Minecraft Idiot");
				MainForm.alreadyAttemptedInject = false;
				OpenMC = 2;
				return;
			}
            {
				string Idiot = "LOGGER\nOpening MC cuz this Idiot is Lazy";
                Console.WriteLine(Idiot);
				panel.Width = 0;
				label.Text = "Opening Minecraft...";
				Form1.Refresh();
				MessageBox.Show("Why are you so lazy, you should be able to open it anyways, here. I will open it for you");
				Process.Start("minecraft://");
                OpenMC = 3;
				Thread.Sleep(200);
                OpenMC = 0;
			}
			MainForm.applyAppPackages(DownloadedDllFilePath);
			Process processesByName = Process.GetProcessesByName("Minecraft.Windows")[0];
			IntPtr intPtr = MainForm.OpenProcess(1082, false, processesByName.Id);
			IntPtr procAddress = MainForm.GetProcAddress(MainForm.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
			IntPtr intPtr1 = MainForm.VirtualAllocEx(intPtr, IntPtr.Zero, (uint)((DownloadedDllFilePath.Length + 1) * Marshal.SizeOf(typeof(char))), 12288, 4);
			MainForm.WriteProcessMemory(intPtr, intPtr1, Encoding.Default.GetBytes(DownloadedDllFilePath), (uint)((DownloadedDllFilePath.Length + 1) * Marshal.SizeOf(typeof(char))), out uIntPtr);
			MainForm.CreateRemoteThread(intPtr, IntPtr.Zero, 0, procAddress, intPtr1, 0, IntPtr.Zero);
			MainForm.alreadyAttemptedInject = false;
			label.Text = "Injected!";
			Form1.Refresh();
		}

		private void InjectDllButton_Click(object sender, EventArgs e)
		{
			this.InjectDllButton.BackColor = Color.FromArgb(44, 44, 44);
			MainForm.InjectDLL(this.DllPath, this.DevInfoText, this.uselessthingjustneededfortheinjectfunction, this);
		}

		public static bool InternetCheck()
		{
			int num;
			if (MainForm.InternetGetConnectedState(out num, 0) && MainForm.OnlineCheck())
			{
				return true;
			}
			return false;
		}

		[DllImport("wininet.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

		private void label2_Click(object sender, EventArgs e)
		{
			Process.Start("https://discord.gg/sJ8D62ejQf");
		}

		private void LocateDllButton_Click(object sender, EventArgs e)
		{
			this.LocateDllButton.BackColor = Color.FromArgb(44, 44, 44);
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (openFileDialog.SafeFileName.ToLower().EndsWith(".dll"))
				{
					this.DllPath = openFileDialog.FileName;
					this.DevInfoText.Text = "Dll Selected";
					this.Refresh();
					return;
				}
				this.DevInfoText.Text = "No Dll Selected";
				this.Refresh();
			}
		}

		private void MainForm_Activated(object sender, EventArgs e)
		{
			this.SetBorderColor(this.ActiveBorderColor);
			this.SetTextColor(this.ActiveTextColor);
		}

		private void MainForm_Deactivate(object sender, EventArgs e)
		{
			this.SetBorderColor(this.InactiveBorderColor);
			this.SetTextColor(this.InactiveTextColor);
		}

		private void MinimizeLabel_Click(object sender, EventArgs e)
		{
			base.WindowState = FormWindowState.Minimized;
		}

		public static bool OnlineCheck()
		{
			bool statusCode;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://github.com/");
			httpWebRequest.Timeout = 15000;
			httpWebRequest.Method = "HEAD";
#pragma warning disable CS0168 // Variable is declared but never used
            try
			{
				using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					statusCode = response.StatusCode == HttpStatusCode.OK;
				}
			}
			catch (WebException webException)
			{
				statusCode = false;
			}
#pragma warning restore CS0168 // Variable is declared but never used
            return statusCode;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		protected void SetBorderColor(Color color)
		{
			this.TopBorderPanel.BackColor = color;
			this.LeftBorderPanel.BackColor = color;
			this.RightBorderPanel.BackColor = color;
			this.BottomBorderPanel.BackColor = color;
		}

		protected void SetLabelBackgroundOnly(Control control, MainForm.MouseState state)
		{
			if (!base.ContainsFocus)
			{
				return;
			}
			Color activeTextColor = this.ActiveTextColor;
			Color normalBackColor = this.NormalBackColor;
			if (state == MainForm.MouseState.Hover)
			{
				normalBackColor = this.HoverBackColor;
			}
			else if (state == MainForm.MouseState.Down)
			{
				normalBackColor = this.DownBackColor;
			}
			control.ForeColor = activeTextColor;
			control.BackColor = normalBackColor;
		}

		protected void SetLabelColors(Control control, MainForm.MouseState state)
		{
			if (!base.ContainsFocus)
			{
				return;
			}
			Color activeTextColor = this.ActiveTextColor;
			Color normalBackColor = this.NormalBackColor;
			if (state == MainForm.MouseState.Hover)
			{
				activeTextColor = this.HoverTextColor;
				normalBackColor = this.HoverBackColor;
			}
			else if (state == MainForm.MouseState.Down)
			{
				activeTextColor = this.DownTextColor;
				normalBackColor = this.DownBackColor;
			}
			control.ForeColor = activeTextColor;
			control.BackColor = normalBackColor;
		}

		protected void SetTextColor(Color color)
		{
			this.TitleLabel.ForeColor = color;
			this.MinimizeLabel.ForeColor = color;
			this.CloseLabel.ForeColor = color;
		}

		private void startDownload(string toDownload, string saveLocation)
		{
			(new Thread(() => {
				WebClient webClient = new WebClient();
				webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.client_DownloadProgressChanged);
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.client_DownloadFileCompleted);
				webClient.DownloadFileAsync(new Uri(toDownload), saveLocation);
			})).Start();
		}

		private void TitleLabel_MouseDown(object sender, MouseEventArgs e)
		{
			this.drag = true;
			this.start_point = new Point(e.X, e.Y);
		}

		private void TitleLabel_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.drag)
			{
				Point screen = base.PointToScreen(e.Location);
				base.Location = new Point(screen.X - this.start_point.X, screen.Y - this.start_point.Y);
			}
		}

		private void TitleLabel_MouseUp(object sender, MouseEventArgs e)
		{
			this.drag = false;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=true, SetLastError=true)]
		private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

		public enum MouseState
		{
			Normal,
			Hover,
			Down
		}

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void TitleLabel_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
			Process.Start("https://discord.gg/Packet");
		}

        private void pictureBox5_Click(object sender, EventArgs e)
        {
			Process.Start("https://discord.gg/Packet");
		}

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Home_Click(object sender, EventArgs e)
        {

        }
    }
}
