import os

# Set the directory to be analyzed
directory = '.'

# Set the file extensions to be included
extensions = ['.cs','.py', '.html', '.js', '.php', '.txt']

# Set the list of directories to exclude
exclude_dirs = ['styles', 'fpdf', 'email', 'image']

image_extensions = ['.png','.jpg','.giff']
# Initialize a counter for the total number of lines
total_lines = 0
total_files = 0
total_images = 0
# Initialize a dictionary to store the number of lines for each file
lines_dict = {}

# Iterate through all subdirectories and files in the directory
for subdir, dirs, files in os.walk(directory):
    # Skip over any excluded directories
    dirs[:] = [d for d in dirs if d not in exclude_dirs]
    for file in files:
        # Check if the file has one of the allowed extensions
        if os.path.splitext(file)[1] in extensions:
            total_files += 1
            # Get the full file path
            file_path = os.path.join(subdir, file)
            # Open the file and count the number of lines
            with open(file_path, 'r') as f:
                file_lines = len(f.readlines())
                print(file)
                print("    ",file_lines)
                total_lines += file_lines
        if os.path.splitext(file)[1] in image_extensions:
            total_images += 1

print(f'Total lines of code: {total_lines}')
print(f'Total files: {total_files}')
print(f'Total images: {total_images}')
