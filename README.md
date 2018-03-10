# CppMerger
Merge all C++ header and source files into a single file and generate one cpp file for most likely coding game platform(www.codingame.com)


This program will traverse source directory, checks all include dependencies and create/append into new cpp file indicated as target path with the including order. This cpp file checks only syntax errors so that output file does not look good enough yet. 

To do : 
- creates option parameter for search directory and output directory
- find search directory recursively
- reorganize #include header file
- update usage example
