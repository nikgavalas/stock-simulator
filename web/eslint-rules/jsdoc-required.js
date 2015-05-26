/**
 * @fileoverview Rule to enforce having jsdoc comments for functions
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
	 * Checks if there is a jsdoc comment for the function node.
	 * @param {ASTNode} node The node of a function block assignment
	 * @returns {void} undefined
	 **/
	function checkForJsDoc(node) {
		var jsdoc = context.getJSDocComment(node);
		if (!jsdoc) {
			context.report(node, 'A valid JSDoc comment is required for functions.');
		}
	}

	return {
		'FunctionDeclaration': checkForJsDoc,

		/**
		 * Only calls the check if this function is being assigned to a var
		 * @param  {Object} node Node in question
		 * @returns {void}
		 */
		'FunctionExpression': function(node) {
			if (isFunctionAssignedToVar(node)) {
				checkForJsDoc(node);
			}
		}
		
	};

};
