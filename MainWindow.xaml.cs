using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EZGitUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "info.json"))
            {
                try
                {
                    var str = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "info.json");
                    var info = JsonConvert.DeserializeObject<GithubPersonalInfo>(str);
                    txtRepo.Text = info.RepoName;
                    txtToken.Text = info.Token;
                    txtUserName.Text = info.UserName;
                }
                catch
                {

                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtRepo.Text.Trim())) {
                MessageBox.Show("Repo Name is required");
                return;
            }
            if (string.IsNullOrEmpty(txtUserName.Text.Trim()))
            {
                MessageBox.Show("User Name is required");
                return;
            }

            if (string.IsNullOrEmpty(txtToken.Text.Trim()))
            {
                MessageBox.Show("Token is required");
                return;
            }

            btnSave.IsEnabled = false;
            btnSave.Content = "Checking...";

            try
            {

                var client = new GitHubClient(new ProductHeaderValue("DONMATEST"));

                //從網站上取得的 personal access token https://github.com/settings/tokens 
                var tokenAuth = new Credentials(txtToken.Text); // NOTE: not real token

                client.Credentials = tokenAuth;

                var repository = client.Repository.GetAllForUser(txtUserName.Text).Result;

                bool isHitRepo=false;

                foreach (var repo in repository)
                {
                    if (repo.Name == txtRepo.Text.Trim())
                    {
                        var info = new GithubPersonalInfo {  RepoName=txtRepo.Text , Token=txtToken.Text , UserName=txtUserName.Text,RepoId=repo.Id};

                        var str = JsonConvert.SerializeObject(info);
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "info.json", str);

                        GithubDataWindow newWindows = new GithubDataWindow();
                        newWindows.Title = txtUserName.Text + " -> " + txtRepo.Text;
                        newWindows.GitInfo = info;
                        this.Hide();
                        newWindows.ShowDialog();
                        isHitRepo = true;
                        break;
                    }
                }

                if (!isHitRepo) {
                    MessageBox.Show("Error: I can find the Repo");
                    btnSave.IsEnabled = true;
                    btnSave.Content = "Test And Save";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message);
            }



       
        }

      
    }
}
