using System;
using System.Collections.Generic;
using System.Text;

namespace EZGitUploader
{
    public class GithubPersonalInfo
    {
        public string RepoName { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }

        public long RepoId { get; set; }
    }
}
