import os
import argparse

def generate_tree(root_dir, exclude_dirs=None, output_file='project_tree_and_files.txt', max_depth=None, include_extensions=None):
    """
    Generates a tree structure of the directory and writes it to a text file.
    
    :param root_dir: The root directory to start the tree from.
    :param exclude_dirs: A set of directory names to exclude from the tree.
    :param output_file: The file path to write the tree structure.
    :param max_depth: Maximum depth to traverse. None for unlimited.
    :param include_extensions: Set of file extensions to include in the tree. None for all.
    """
    if exclude_dirs is None:
        exclude_dirs = set()
    if include_extensions is not None:
        include_extensions = set(ext.lower() for ext in include_extensions)

    with open(output_file, 'w', encoding='utf-8') as f:
        root_name = os.path.basename(os.path.abspath(root_dir))
        if not root_name:
            root_name = root_dir
        f.write(root_name + '\n')
        _generate_tree_recursive(
            current_dir=root_dir,
            prefix='',
            exclude_dirs=exclude_dirs,
            file_handle=f,
            current_depth=0,
            max_depth=max_depth,
            include_extensions=include_extensions
        )

    print(f"Directory tree has been written to '{output_file}'.")

def _generate_tree_recursive(current_dir, prefix, exclude_dirs, file_handle, current_depth, max_depth, include_extensions):
    """
    Helper function to recursively generate the tree structure.
    """
    if max_depth is not None and current_depth >= max_depth:
        return

    try:
        entries = sorted(os.listdir(current_dir))
    except PermissionError:
        file_handle.write(prefix + '└── ' + '[Permission Denied]\n')
        return
    except FileNotFoundError:
        file_handle.write(prefix + '└── ' + '[Not Found]\n')
        return

    # Exclude specified directories
    entries = [e for e in entries if e not in exclude_dirs]
    entries_count = len(entries)

    for index, entry in enumerate(entries):
        path = os.path.join(current_dir, entry)
        connector = '├── ' if index < entries_count - 1 else '└── '
        line = prefix + connector + entry

        # If filtering by file extension, skip files that do not match.
        if os.path.isfile(path) and include_extensions:
            ext = os.path.splitext(entry)[1].lower()
            if ext not in include_extensions:
                continue

        file_handle.write(line + '\n')

        if os.path.isdir(path):
            extension = '│   ' if index < entries_count - 1 else '    '
            _generate_tree_recursive(
                current_dir=path,
                prefix=prefix + extension,
                exclude_dirs=exclude_dirs,
                file_handle=file_handle,
                current_depth=current_depth + 1,
                max_depth=max_depth,
                include_extensions=include_extensions
            )

def combine_files(directory, output_file, mobile_extensions, mobile_image_extensions, exclude_dirs):
    """
    Walks through the directory tree and appends mobile app source files' contents to the output file.
    The file contents are appended as-is (including comments and import/require statements).
    
    :param directory: The root directory to search.
    :param output_file: The file to append the source file contents.
    :param mobile_extensions: Set of file extensions for mobile app files.
    :param mobile_image_extensions: Set of image file extensions to count (but not append).
    :param exclude_dirs: A set of directory names to exclude.
    """
    total_files = 0
    total_images = 0

    # Open in append mode so the tree structure remains at the top.
    with open(output_file, 'a', encoding='utf-8') as outfile:
        # Write a header separator before the combined files content.
        outfile.write("\n" + "=" * 40 + "\n")
        outfile.write("Combined Mobile App Files:\n")
        outfile.write("=" * 40 + "\n")

        for subdir, dirs, files in os.walk(directory):
            # Exclude directories that we do not want to process.
            dirs[:] = [d for d in dirs if d not in exclude_dirs]
            for file in files:
                file_extension = os.path.splitext(file)[1].lower()
                file_path = os.path.join(subdir, file)

                if file_extension in mobile_extensions:
                    total_files += 1
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            content = f.read()
                            relative_path = os.path.relpath(file_path, directory)
                            separator = f"\n// File: {relative_path}\n"
                            outfile.write(separator)
                            outfile.write(content + "\n")
                    except Exception as e:
                        print(f"Error reading {file_path}: {e}")
                elif file_extension in mobile_image_extensions:
                    total_images += 1

    print(f"All mobile app files have been combined into '{output_file}'.")
    print(f"Total files processed: {total_files}")
    print(f"Total images found: {total_images}")

def main():
    parser = argparse.ArgumentParser(
        description='Generate a directory tree and combine mobile app files (with comments and imports) into one output file.'
    )
    parser.add_argument(
        'directory',
        nargs='?',
        default='.',
        help='The root directory to process. Defaults to the current directory.'
    )
    parser.add_argument(
        '-o', '--output',
        default='project_tree_and_files.txt',
        help='Output file to write the tree structure and combined file contents. Defaults to "project_tree_and_files.txt".'
    )
    parser.add_argument(
        '-e', '--exclude',
        nargs='*',
        default=[],
        help='List of additional directories to exclude (e.g., styles, fpdf, email, image).'
    )
    parser.add_argument(
        '-d', '--depth',
        type=int,
        default=None,
        help='Limit the depth of the tree. Defaults to no limit.'
    )
    parser.add_argument(
        '-f', '--file-types',
        nargs='*',
        default=None,
        help='List of file extensions to include in the tree (e.g., .cs .py). If not specified, all files are included.'
    )
    parser.add_argument(
        '--mobile-file-types',
        nargs='*',
        default=['.js', '.jsx', '.ts', '.tsx', '.json'],
        help='List of file extensions for mobile app files to combine. Defaults to ".js .jsx .ts .tsx .json".'
    )
    parser.add_argument(
        '--mobile-image-types',
        nargs='*',
        default=['.png', '.jpg', '.gif', '.jpeg', '.svg'],
        help='List of image file extensions to count (not combine). Defaults to ".png .jpg .gif .jpeg .svg".'
    )

    args = parser.parse_args()
    root_dir = os.path.abspath(args.directory)

    # Default directories to exclude.
    default_exclusions = {
        'bin', '.vs', 'obj', 'node_modules', '.git', '.expo',
        '.vscode', '__pycache__', 'fpdf', 'email', 'image', 'assets'
    }
    if args.exclude:
        default_exclusions.update(args.exclude)

    output_file = args.output
    max_depth = args.depth
    tree_include_extensions = set(ext.lower() for ext in args.file_types) if args.file_types else None
    mobile_extensions = set(ext.lower() for ext in args.mobile_file_types)
    mobile_image_extensions = set(ext.lower() for ext in args.mobile_image_types)

    if not os.path.isdir(root_dir):
        print(f"Error: The directory '{root_dir}' does not exist or is not a directory.")
        return

    # Generate the directory tree and write it to the output file.
    generate_tree(
        root_dir=root_dir,
        exclude_dirs=default_exclusions,
        output_file=output_file,
        max_depth=max_depth,
        include_extensions=tree_include_extensions
    )

    # Append the combined mobile app files (with original content) to the same output file.
    combine_files(
        directory=root_dir,
        output_file=output_file,
        mobile_extensions=mobile_extensions,
        mobile_image_extensions=mobile_image_extensions,
        exclude_dirs=default_exclusions
    )

if __name__ == '__main__':
    main()
