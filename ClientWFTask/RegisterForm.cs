using ClientWFTask.Services;
using GameCore;

namespace ClientWFTask;

public partial class RegisterForm : Form
{
    public RegisterForm() => InitializeComponent();

    private void btnCreate_Click(object sender, EventArgs e)
    {
        GameConnection.Instance.PacketReceived += OnRegistered;
        GameConnection.Instance.Send(new RegisterManagePacket
        {
            Username = txtLogin.Text.Trim(),
            Password = txtPassword.Text,
            IsRegister = true
        });
    }

    private void OnRegistered(INetworkPacket packet)
    {
        if (packet is not LoginResultPacket result) return;
        GameConnection.Instance.PacketReceived -= OnRegistered;
        if (InvokeRequired) { BeginInvoke(() => OnRegistered(packet)); return; }

        MessageBox.Show(result.Message, "Регистрация");
        if (result.Success) Close();
    }
}
