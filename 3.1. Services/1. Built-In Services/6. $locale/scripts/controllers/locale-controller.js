app.controller('LocaleController', 
	function LocaleController($scope, $locale) {
		$scope.date = Date.now();
		$scope.format = $locale.DATETIME_FORMATS.fullDate;
		$scope.format2 = $locale.DATETIME_FORMATS.shortDate;
		
		console.log($locale); // See all date time formats
});