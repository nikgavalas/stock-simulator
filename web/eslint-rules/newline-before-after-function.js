/**
 * @fileoverview Rule to enforce spaces before and after function blocks
 * @author Nikolas Gavalas
 * @copyright 2015 Stilman Advanced Strategies, LLC. All rights reserved.
 */
'use strict';

//------------------------------------------------------------------------------
// Rule Definition
//------------------------------------------------------------------------------

/**
 * Exported module for ESLint
 * @param  {Object} context Context object
 * @returns {Object} Map of tokens to functions
 */
module.exports = function(context) {

	//---------------------------------------------------------------------
	// Helpers
	//---------------------------------------------------------------------

	/**
	 *  Determines if there is a space directly before a block or if it's a comment
	 *  @param {object} code array of each line of the code
	 *  @param {integer} lineNumber of where the block begins
	 *  @returns {boolean} whether there was a space beforev
	 **/
	function isNewLineBefore(code, lineNumber) {
		// block 'starts' the line after the first '{'
		var lineOfCode = code[lineNumber - 2];
		if (lineOfCode) {
			return lineOfCode.replace('\t', '').trim() === '';
		}

		return true;
	}

	/**
	 *  Determines if there is a space directly after a function block
	 *  @param {object} code array of each line of the code
	 *  @param {integer} lineNumber of where the block begins
	 *  @returns {boolean} whether there was a space beforev
	 **/
	function isNewLineAfter(code, lineNumber) {
		var lineOfCode = code[lineNumber];
		if (lineOfCode) {
			return lineOfCode.replace('\t', '').trim() === '';
		}

		return true;
	}

	/**
	 * Determines if a function is a function that is assigned to a variable. In
	 * that case we'll determine that it is another form of a function declaration.
	 * @param {Object} node Node in question
	 * @returns {Boolean}      True if the function is an argument
	 */
	function isFunctionAssignedToVar(node) {
		var lastToken = context.getTokenBefore(node);
		return lastToken.type === 'Punctuator' && (lastToken.value === '=' || lastToken.value === ':');
	}

	/**
	 * Checks if there is an function block so we can determine if we need
	 * a new line before or after
	 * @param {ASTNode} node The node of a function block assignment
	 * @returns{void} undefined
	 **/
	function checkForNewLineAroundBlock(node) {
		var code, startLineNumber, endLineNumber;
		code = context.getSourceLines();

		// If there is a jsdoc node above the function, use that node to see if there
		// is a newline above that instead of just the function.
		var jsdoc = context.getJSDocComment(node);
		var comment = context.getComments(node);

		startLineNumber = node.loc.start.line;
		if (jsdoc) {
			startLineNumber = jsdoc.loc.start.line;
		}
		else if (comment && comment.leading && comment.leading.length) {
			startLineNumber = comment.leading[0].loc.start.line;	
		}

		endLineNumber = node.loc.end.line;

		if (!isNewLineBefore(code, startLineNumber) && !isNewLineAfter(code, endLineNumber)) {
			context.report(node, 'New Line is required above the function block or JSDoc comment and below a function block.');
		} 
		else if (!isNewLineBefore(code, startLineNumber)) {
			context.report(node, 'New line is required above the function block or JSDoc comment.');
		} 
		else if (!isNewLineAfter(code, endLineNumber)) {
			context.report(node, node.loc.end, 'New line is required below the function block.');
		}
	}

	return {
		'FunctionDeclaration': checkForNewLineAroundBlock,

		/**
		 * Only calls the check if this function is being assigned to a var
		 * @param  {Object} node Node in question
		 * @returns {void}
		 */
		'FunctionExpression': function(node) {
			if (isFunctionAssignedToVar(node)) {
				checkForNewLineAroundBlock(node);
			}
		}
		
	};

};
