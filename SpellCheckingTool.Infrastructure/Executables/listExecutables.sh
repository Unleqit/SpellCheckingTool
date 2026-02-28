#!/bin/bash

(
	source ~/.bashrc
	shopt -s expand_aliases
	PATH=$(printf '%s\n' "$PATH" | tr ':' '\n' | grep -v '^/mnt/' | paste -sd:)
	compgen -b 
	compgen -A alias 

	IFS=':'
	for dir in $PATH; do
		[ -d "$dir" ] || continue
		for file in "$dir"/*; do
			[ -x "$file" ] && echo "$(basename "$file")"
		done
	done
	unset IFS
)