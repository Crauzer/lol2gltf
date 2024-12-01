# Get the latest commit hash (first 7 characters)
$latestCommit = git rev-parse --short=7 HEAD

# Get the current date in YYYY-MM-DD format
$currentDate = Get-Date -Format "yyyy-MM-dd"

# Create a tag name using the date and commit hash
$tagName = "${currentDate}-${latestCommit}"

# Create the tag
git tag $tagName

# Output the tag name for use in GitHub Actions
$tagName