#!/usr/bin/env nu

# Prompt the user for a commit message
echo "Enter commit message: "
let commit_message = (input)

# Run git add .
echo "Running git add ."
git add .

# Run git commit with the user's commit message
echo "Running git commit -m $commit_message"
git commit -m $commit_message

# Run git push
echo "Running git push"
git push
