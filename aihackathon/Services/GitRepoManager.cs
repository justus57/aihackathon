using LibGit2Sharp;
using Octokit;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Signature = LibGit2Sharp.Signature;

namespace CodeOptimizer.Services
{
    public class GitRepoManager
    {
        private readonly string _repourl;
        private readonly string _localPath;
        private readonly string _branchName;
        private readonly string _commitMessage;

        public GitRepoManager(string repourl, string localPath, string newBranchName, string commitMessage)
        {
            _repourl = repourl;
            _localPath = localPath;
            _branchName = newBranchName;
            _commitMessage = commitMessage;
        }

        public void CloneRepo()
        {
            try
            {
                if (Directory.Exists(_localPath))
                {
                    Console.WriteLine($"Repository already exists at: {_localPath}");
                    return;
                }

                Console.WriteLine($"Cloning repository from: {_repourl}");
                LibGit2Sharp.Repository.Clone(_repourl, _localPath);
                Console.WriteLine("Repository cloned successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cloning repository: {ex.Message}");
            }
        }

        public async Task CreateBranch()
        {
            try
            {
                using (var repo = new LibGit2Sharp.Repository(_localPath))
                {
                    LibGit2Sharp.Branch branch;

                    // Check if branch already exists
                    var existingBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName.Equals(_branchName));
                    if (existingBranch != null)
                    {
                        Console.WriteLine($"Branch {_branchName} already exists. Using existing branch.");

                        branch = existingBranch;

                    }
                    else
                    {

                        Console.WriteLine($"Creating new branch: {_branchName}");
                        branch = repo.CreateBranch(_branchName);
                    }
                    repo.Branches.Update(branch,
    b => b.TrackedBranch = $"refs/remotes/origin/{_branchName}");

                    Commands.Checkout(repo, branch);

                    // Check if there are any changes to commit
                    var status = repo.RetrieveStatus();
                    if (status.IsDirty)
                    {
                        Commands.Stage(repo, "*");

                        // Commit changes
                        var author = new Signature("AI Code Optimizer", "ai-optimizer@example.com", DateTimeOffset.Now);
                        repo.Commit(_commitMessage, author, author);
                        Console.WriteLine("Changes committed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No changes to commit.");
                    }

                    // Push to remote
                    try
                    {
                        var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                        if (string.IsNullOrEmpty(gitHubToken))
                        {
                            Console.WriteLine("GitHub token not found. Cannot push to remote.");
                            return;
                        }
                        
                        var pushOptions = new PushOptions
                        {
                            CredentialsProvider = (_url, _user, _cred) =>
                                new UsernamePasswordCredentials
                                {
                                    Username = "justus57",
                                    Password = gitHubToken
                                }
                        };
                        repo.Network.Push(branch, pushOptions);
                        Console.WriteLine($"Branch {_branchName} pushed successfully.");
                    }
                    catch (Exception pushEx)
                    {
                        Console.WriteLine($"Error pushing branch: {pushEx.Message}");
                    }
                }

                Console.WriteLine($"Branch {_branchName} created, committed and pushed successfully. Ready For PR...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating branch: {ex.Message}");
            }
        }

        public async Task<string?> CreatePr()
        {
            try
            {
                var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                if (string.IsNullOrEmpty(gitHubToken))
                {
                    Console.WriteLine("GitHub token not found. Please set GITHUB_TOKEN environment variable.");
                    Console.WriteLine("You can set it by running: $env:GITHUB_TOKEN='your-token-here'");
                    return null;
                }
                
                string owner = "justus57"; // Updated to match your repo
                string repoName = "aihackathon"; // Updated to match your repo

                var prTitle = "AI Code Optimization - Automated Branch";
                var description = "Automated PR created by AI Code Optimizer:\n1. Optimized code for memory efficiency\n2. Applied AI-suggested improvements\n3. Updated with best practices";

                var client = new GitHubClient(new ProductHeaderValue("AI-Code-Optimizer"))
                {
                    Credentials = new Octokit.Credentials(gitHubToken)
                };

                var newPr = new NewPullRequest(prTitle, _branchName, "main")
                {
                    Body = description
                };

                var pr = await client.PullRequest.Create(owner, repoName, newPr);
                Console.WriteLine($"Pull request created successfully: {pr.HtmlUrl}");
                return pr.HtmlUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating pull request: {ex.Message}");
                return null;
            }
        }
    }
}
