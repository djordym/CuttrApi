import os
import re

# Set the directory to be analyzed
directory = '.'

# Set the file extensions to be included for mobile app
extensions = ['.js', '.jsx', '.ts', '.tsx', '.json']

# Set the list of directories to exclude, including 'node_modules'
exclude_dirs = ['node_modules', 'fpdf', 'email', 'image', '.expo', '.git', 'assets']

# Set the image file extensions (if you still need to track images)
image_extensions = ['.png', '.jpg', '.gif', '.jpeg', '.svg']

# Initialize counters
total_files = 0
total_images = 0

# Path for the output file
output_file_path = os.path.join(directory, 'all_mobile_app_files_combined_cleaned.txt')

def remove_comments_whitespace_and_imports(code, file_extension):
    """
    Removes single-line and multi-line comments,
    removes lines containing import/require statements (for JS/TS files),
    and minimizes whitespace in the code.
    """
    # Remove single-line comments (// ...)
    code_no_single_comments = re.sub(r'//.*', '', code)

    # Remove multi-line comments (/* ... */)
    code_no_comments = re.sub(r'/\*.*?\*/', '', code_no_single_comments, flags=re.DOTALL)

    # Split into lines for further processing
    lines = code_no_comments.splitlines()

    cleaned_lines = []
    for line in lines:
        stripped_line = line.strip()
        # Skip empty lines
        if not stripped_line:
            continue

        # Remove lines containing import or require statements
        if re.match(r'^(import|require)\s+', stripped_line):
            continue

        # Optionally, reduce multiple spaces to single spaces within the line
        stripped_line = re.sub(r'\s+', ' ', stripped_line)
        cleaned_lines.append(stripped_line)

    # Join the cleaned lines back into a single string
    cleaned_code = '\n'.join(cleaned_lines)

    return cleaned_code

# Open the output file in write mode
with open(output_file_path, 'w', encoding='utf-8') as outfile:
    # Iterate through all subdirectories and files in the directory
    for subdir, dirs, files in os.walk(directory):
        # Skip over any excluded directories
        dirs[:] = [d for d in dirs if d not in exclude_dirs]
        for file in files:
            file_extension = os.path.splitext(file)[1].lower()
            file_path = os.path.join(subdir, file)

            # Check if the file has one of the allowed extensions
            if file_extension in extensions:
                total_files += 1

                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                        # Remove comments, whitespace, and import/require lines
                        cleaned_content = remove_comments_whitespace_and_imports(content, file_extension)

                        # Extract the relative path for the separator
                        relative_path = os.path.relpath(file_path, directory)
                        separator = f"\n// File: {relative_path}\n"

                        outfile.write(separator)
                        outfile.write(cleaned_content)
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")

            # Check if the file is an image (if you still want to track images)
            elif file_extension in image_extensions:
                total_images += 1

print(f'All mobile app files have been combined and cleaned into {output_file_path}')
print(f'Total files processed: {total_files}')
print(f'Total images found: {total_images}')
