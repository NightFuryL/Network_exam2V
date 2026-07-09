using ClientWFTask.Services;
using DatabaseLibrary.Entities;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class RegisterForm : Form
{
    public RegisterForm()
    {
        InitializeComponent();
        BoardFormHelper.StyleForm(this);
        label1.Text = "Nickname:";
        BoardFormHelper.StyleLabel(label1, true);
        btnCreate.Text = "Enter";
        BoardFormHelper.StyleFormButton(btnCreate);
        BoardFormHelper.StyleFormButton(btnWatch);
        txtLogin.BackColor = Color.FromArgb(10, 30, 72);
        txtLogin.ForeColor = BoardFormHelper.AppText;
        txtLogin.BorderStyle = BorderStyle.FixedSingle;
    }

    private async void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            string nick = txtLogin.Text.Trim();
            if (string.IsNullOrWhiteSpace(nick))
            {
                MessageBox.Show("Enter nickname");
                return;
            }

            GameConnection.Instance.EnsureConnected();
            var packet = new NetworkPacket
            {
                CommandCode = PacketType.LoginAndRegister,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new LoginRequest { Username = nick }))
            };
            var response = await GameConnection.Instance.SendAndWaitAsync(packet, PacketType.LoginAndRegister);
            if (response.PResult != PacketResult.Success)
            {
                MessageBox.Show("Nickname is already online or login failed");
                return;
            }
            var user = JsonSerializer.Deserialize<UserEntity>(response.Data);
            if (user == null) return;
            Session.SetUser(user);
            Hide();
            new MainForm().ShowDialog();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void btnWatch_Click(object sender, EventArgs e)
    {
        try
        {
            GameConnection.Instance.EnsureConnected();
            new WatchForm().ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
