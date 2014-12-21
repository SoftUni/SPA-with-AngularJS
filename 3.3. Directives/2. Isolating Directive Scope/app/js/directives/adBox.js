app.directive('adBox', function () {
	return {
		restrict: 'A',
		replace: true,
		templateUrl: '/templates/directives/adBox.html',
		scope: {
			ad: "="
		}
	}
})