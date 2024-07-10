#!/usr/bin/env nu

# Read and process input.txt
let processed = (
    open input.txt
    | lines
    | parse "{key} = {value}"
    | update key { |it| $it.key | str upcase | str replace " " "" }
    | update value { |it| $it.value | str trim }
)

# Read existing .env file
let existing_env = (
    if ($".env" | path exists) {
        open .env | lines
    } else {
        []
    }
)

# Remove existing entries and prepare new content
let new_content = (
    $existing_env
    | where { |line|
        let key = ($line | split row "=" | first)
        not ($processed | any { |item| $item.key == $key })
    }
)

# Append new entries
let updated_content = (
    $new_content
    | append ($processed | each { |item| $"($item.key)=($item.value)" })
)

# Write updated content to .env
$updated_content | str join "\n" | save -f .env
