import dashboardService from "../services/dashboard.js";
import spaceService from "../services/spaceService.js";
import ticketService from "../services/tikcetService.js";
import userService from "../services/userService.js";

const API = {
  userService: userService,
  spaceService: spaceService,
  dashboard: dashboardService,
  ticket: ticketService,
};

export default API;
