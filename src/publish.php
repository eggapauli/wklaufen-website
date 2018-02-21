<?php
$packageName = $_GET["package-name"];
if (!preg_match("/^[a-z0-9\-.]+$/", $packageName))
{
    http_response_code(404);
    die("Invalid package name");
}
$packagePath = "../tmp/deploy/" . $packageName;

$zip = new ZipArchive;
$res = $zip->open($packagePath);
if ($res !== TRUE)
{
    http_response_code(500);
    die("Can't open package: Code " . $res);
}

$targetPath = "../web/a/";
$files = new RecursiveIteratorIterator(new RecursiveDirectoryIterator($targetPath), RecursiveIteratorIterator::CHILD_FIRST);

foreach ($files as $file)
{
    if (in_array($file->getBasename(), array(".", "..")) === true)
    {
        continue;
    }
    if ($file->isDir() === true)
    {
        rmdir($file->getPathname());
    }
    else if ($file->isFile() === true || $file->isLink() === true)
    {
        unlink($file->getPathname());
    }
}

$zip->extractTo($targetPath);
$zip->close();

unlink($packagePath);
?>