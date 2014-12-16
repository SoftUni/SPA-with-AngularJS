app.controller('SoftUniApp', function($scope,$http) {


		$scope.obj = {
			color: 'red',
			fontSize: '25px'
		}

		$scope.name = 'Soft Uni App';

		var cars = [{
			model: 'Opel',
			color: 'Pink',
			year: 1986
		},
		{
			model: 'BMW',
			color: 'Green',
			year: 2014
		},{
			model: 'Mercedes',
			color: 'White',
			year: 1900	
		}
		];
		$scope.cars = cars;

		$scope._ngClassOdd = 'pesho';

		$http.get("http://www.w3schools.com/website/Customers_JSON.php")
		.success(function(resp) {
			//Obrabotvame scope
			$scope._resp_ = resp;
		})

		$scope.class = 'even_row'

})
