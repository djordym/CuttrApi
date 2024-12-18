import os
import argparse

def generate_tree(root_dir, exclude_dirs=None, output_file='project_tree.txt', max_depth=None, include_extensions=None):
    """
    Generates a tree structure of the directory and writes it to a text file.

    :param root_dir: The root directory to start the tree from.
    :param exclude_dirs: A set of directory names to exclude from the tree.
    :param output_file: The file path to write the tree structure.
    :param max_depth: Maximum depth to traverse. None for unlimited.
    :param include_extensions: Set of file extensions to include. None for all.
    """
    if exclude_dirs is None:
        exclude_dirs = set()
    if include_extensions is not None:
        include_extensions = set(ext.lower() for ext in include_extensions)

    with open(output_file, 'w', encoding='utf-8') as f:
        root_name = os.path.basename(os.path.abspath(root_dir))
        if root_name == '':
            root_name = root_dir
        f.write(root_name + '\n')
        _generate_tree_recursive(root_dir, prefix='', exclude_dirs=exclude_dirs, file_handle=f, current_depth=0, max_depth=max_depth, include_extensions=include_extensions)

    print(f"Directory tree has been written to '{output_file}'.")

def _generate_tree_recursive(current_dir, prefix, exclude_dirs, file_handle, current_depth, max_depth, include_extensions):
    """
    Helper function to recursively generate the tree structure.

    :param current_dir: The current directory being traversed.
    :param prefix: The prefix string for the current level in the tree.
    :param exclude_dirs: A set of directory names to exclude from the tree.
    :param file_handle: The file handle to write the tree structure.
    :param current_depth: Current depth in the directory tree.
    :param max_depth: Maximum depth to traverse. None for unlimited.
    :param include_extensions: Set of file extensions to include. None for all.
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

    entries = [e for e in entries if e not in exclude_dirs]
    entries_count = len(entries)

    for index, entry in enumerate(entries):
        path = os.path.join(current_dir, entry)
        connector = '├── ' if index < entries_count - 1 else '└── '
        line = prefix + connector + entry

        # File type filtering
        if os.path.isfile(path) and include_extensions:
            if not os.path.splitext(entry)[1].lower() in include_extensions:
                continue

        file_handle.write(line + '\n')

        if os.path.isdir(path):
            extension = '│   ' if index < entries_count - 1 else '    '
            _generate_tree_recursive(
                path,
                prefix + extension,
                exclude_dirs,
                file_handle,
                current_depth + 1,
                max_depth,
                include_extensions
            )

def main():
    parser = argparse.ArgumentParser(description='Generate a directory tree and write it to a text file.')
    parser.add_argument(
        'directory',
        nargs='?',
        default='.',
        help='The root directory to generate the tree from. Defaults to the current directory.'
    )
    parser.add_argument(
        '-o', '--output',
        default='project_tree.txt',
        help='Output file to write the tree structure. Defaults to "project_tree.txt".'
    )
    parser.add_argument(
        '-e', '--exclude',
        nargs='*',
        default=[],
        help='List of additional directories to exclude from the tree (e.g., styles fpdf email image).'
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
        help='List of file extensions to include (e.g., .cs .py). If not specified, all files are included.'
    )

    args = parser.parse_args()
    root_dir = os.path.abspath(args.directory)

    # Default directories to exclude, including 'node_modules'
    default_exclusions = {'bin', '.vs', 'obj', 'node_modules', '.git', '.expo', '.vscode', '__pycache__'}

    # Add any additional exclusions specified via command-line
    if args.exclude:
        default_exclusions.update(args.exclude)

    output_file = args.output
    max_depth = args.depth
    include_extensions = set(args.file_types) if args.file_types else None

    if not os.path.isdir(root_dir):
        print(f"Error: The directory '{root_dir}' does not exist or is not a directory.")
        return

    generate_tree(
        root_dir,
        exclude_dirs=default_exclusions,
        output_file=output_file,
        max_depth=max_depth,
        include_extensions=include_extensions
    )

if __name__ == '__main__':
    main()
