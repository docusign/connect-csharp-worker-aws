# Contributing to eg-01-csharp-jwt-core and -framework

This repo, [eg-01-csharp-jwt-common](https://github.com/docusign/eg-01-csharp-jwt-common)
contains the files that are common to both the `core` and `framework` Visual Studio
projects:
 
* [eg-01-csharp-jwt-core](https://github.com/docusign/eg-01-csharp-jwt-core)
* [eg-01-csharp-jwt-framework](https://github.com/docusign/eg-01-csharp-jwt-framework)

## Git Subtree
The [Git subtree](https://www.atlassian.com/blog/git/alternatives-to-git-submodule-git-subtree)
feature is used to integrate the -common repo with the -core and -framework repos.

The Git Subtree benefit is that read-only users of the repos do not need any additional
commands beyond the usual git clone process to use the repos. Cloning the -core or
-framework repos results in a complete local repo which can be immediately used
with Visual Studio.

## Contributing to the repos
However, the Git Subtree feature makes updates and contributions more complicated:

### Contributing -core or -framework files
If the contribution is **not** to the `/common` files, then the usual git process
should be used:

1. Clone and Branch either eg-01-csharp-jwt-core or eg-01-csharp-jwt-framework
1. Update the file(s)
1. Test and then commit the change.
1. Submit a Pull Request to either eg-01-csharp-jwt-core or eg-01-csharp-jwt-framework

### Contributing -common files

In this case, contributions must be to the -common repo, but they must be 
tested with -core and -framework.

1. Clone and Branch eg-01-csharp-jwt-common
   For this example, our remote branch of the -common repo is
   in https://github.com/myName/eg-01-csharp-jwt-common, branch fixIssue03
1. Also clone eg-01-csharp-jwt-core and eg-01-csharp-jwt-framework
1. After making the fix, commit it to your remote, upstream
   server (https://github.com/myName/eg-01-csharp-jwt-common)
1. Test with the -core repository by:
   1. cd eg-01-csharp-jwt-core
   1. `git subtree pull --prefix=common https://github.com/myName/eg-01-csharp-jwt-common fixIssue03 --squash`
   1. This subtree command will update your local -core repository's /common 
      directory with your fixes
1. After your tests succeed, repeat the prior step for the -framework repo.
1. Success! Create a pull request from https://github.com/myName/eg-01-csharp-jwt-common, branch fixIssue03

   to https://github.com/docusign/eg-01-csharp-jwt-common, branch Master.
   
### Uploading -common changes from the -core or -framework repos

**This flow is not recommended due to its additional complexity.**
 
In this example we'll make changes from the -core repo and then upload 
them to the -common repo

1. Clone the -core repo. Branching is optional since we will not be doing a 
   Pull Request from this repo.
1. Clone and branch the -common repo. You only need a remote repo.
   For this example, our remote branch of the -common repo is
   in https://github.com/myName/eg-01-csharp-jwt-common, branch fixIssue03
1. Make changes to /common files from within the -core repo.
   
   Do NOT make changes to any files unless they are in /common
1. The changes can also be tested from within the -core repo.
1. When you are ready to push your /common changes to the -common repo:
   1. You will need to undo any changes to any files that are not in /common.
   
   Use the `git status`, `git log`, and git reset commands to undo any changes
   as needed.
   1. Add your remote clone of the -common directory as a `remote` for the -core repo:
      `git remote add -f remote-common https://github.com/myName/eg-01-csharp-jwt-common`
   1. Push your subtree changes upstream to your -common clone:
      `git subtree push --prefix common remote-common fixIssue03`
1. Test your changes in the -framework repo. Use the steps from above
   with the `git subtree pull` command to pull your -common changes into
   your local copy of -framework.
1. Success! Create a pull request from https://github.com/myName/eg-01-csharp-jwt-common, branch fixIssue03

   to https://github.com/docusign/eg-01-csharp-jwt-common, branch Master.






