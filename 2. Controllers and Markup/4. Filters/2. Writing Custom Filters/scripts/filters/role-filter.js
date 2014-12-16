app.filter('role', function role() {
	return function (role) {
		switch(role) {
			case 1: return 'Administrator';
			case 2: return 'Copywriter';
			case 3: return 'Moderator';
			default: return 'User';
		}
	}
})