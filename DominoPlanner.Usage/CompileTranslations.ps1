$locales = @("de-DE")
$poFiles = $($locales | ForEach-Object { "locale/" + $_ + "/LC_MESSAGES/DominoPlanner.po" })

$locales | ForEach-Object {
	$file = "locale/" + $_ + "/LC_MESSAGES/DominoPlanner.po"
	echo $file
	$output = "locale/" + $_ + "/LC_MESSAGES/DominoPlanner.mo"
	msgfmt.exe $file -o $output
}