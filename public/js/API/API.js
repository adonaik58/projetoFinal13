import dashboardService from "../services/dashboard.js";
import spaceService from "../services/spaceService.js";
import userService from "../services/userService.js";

const API = {
  userService: userService,
  spaceService: spaceService,
  dashboard: dashboardService,
};

export default API;
