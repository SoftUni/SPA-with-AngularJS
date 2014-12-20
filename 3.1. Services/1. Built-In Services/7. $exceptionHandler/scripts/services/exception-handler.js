app.factory('$exceptionHandler', function() {
	return function (exception) {
		console.log("Custom exception handler: " + exception.message);
	};
});