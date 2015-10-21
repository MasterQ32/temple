# temple

Temple is a simple templating language generation files from other files. It allows scripting file generation with the system installation of Lua.

## Command Line Usage
`temple`
Reads a file from stdin, writes the output to stdout.

`temple file`
Reads the file from file, writes the output to stdout.

`temple infile outfile`
Reads the file from infile, writes the output to outfile.

## Templating
temple uses a similar system to php:
Copying the file as is for everything except text between `<?` and `?>`.
Everything that is written between `<?` and `?>` is interpreted as Lua code that is executed when the temple is beeing run.
It is possible to wrap plain text into lua snippets. Each plain text part is replaced by `print("escaped string literal")`

## Examples

### Creating a list from 1 to 10
	List:
	<? for i=1,10 do ?>
	Item: <? print(i) ?>
	<? end ?>

### #include "file.h"
	<?
		f = io.open("file.h")
		print(f:read("*a"))
		f:close()
	?>

	int main() {
		return 0;
	}

## Planned Features
Those features may be implemented in the far future or when someone does a feature request
- using `<?= expr ?>` for direct printing
- ignoring whitespace between `?>` and `<?` if there is only whitespace
- adding command line options with -o for output and "raw" file parameters will be concatenated.
