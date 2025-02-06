import os
import re

# Set the directory to be analyzed
directory = '.'

# Set the file extensions to be included
extensions = ['.cs', '.py', '.html', '.js', '.php', '.txt']

# Set the list of directories to exclude
exclude_dirs = ['styles', 'fpdf', 'email', 'image', 'Migrations']

# Set the image file extensions
image_extensions = ['.png', '.jpg', '.giff']

# Initialize counters
total_files = 0
total_images = 0

# Path for the output file
output_file_path = os.path.join(directory, 'all_cs_files_combined_cleaned.cs')

def remove_comments_whitespace_and_usings(code):
    """
    Removes single-line and multi-line comments,
    removes lines containing 'using',
    and minimizes whitespace in C# code.
    """
    # Remove single-line comments
    code_no_single_comments = re.sub(r'//.*', '', code)

    # Remove multi-line comments
    code_no_comments = re.sub(r'/\*.*?\*/', '', code_no_single_comments, flags=re.DOTALL)

    # Split into lines for further processing
    lines = code_no_comments.splitlines()

    cleaned_lines = []
    for line in lines:
        stripped_line = line.strip()
        # Skip empty lines
        if not stripped_line:
            continue
        # Skip lines containing 'using'
        if 'using' in stripped_line:
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

                # If the file is a .cs file, process its contents
                if file_extension == '.cs':
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            content = f.read()
                            # Remove comments, whitespace, and lines containing 'using'
                            cleaned_content = remove_comments_whitespace_and_usings(content)

                            # Extract the base filename without extension for the separator
                            base_filename = os.path.splitext(os.path.basename(file))[0]
                            separator = f"\n//{base_filename}\n"

                            outfile.write(separator)
                            outfile.write(cleaned_content)
                    except Exception as e:
                        print(f"Error reading {file_path}: {e}")

            # Check if the file is an image
            if file_extension in image_extensions:
                total_images += 1

print(f'All .cs files have been combined and cleaned into {output_file_path}')
print(f'Total files processed: {total_files}')
print(f'Total images found: {total_images}')
