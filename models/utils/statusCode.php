<?php

/** HTTP status codes */
class HttpStatusCode {

    protected const SWITCHING_PROTOCOLS = 101;
    protected const OK = 200;
    protected const CREATED = 201;
    protected const ACCEPTED = 202;
    protected const NONAUTHORITATIVE_INFORMATION = 203;
    protected const NO_CONTENT = 204;
    protected const RESET_CONTENT = 205;
    protected const PARTIAL_CONTENT = 206;
    protected const MULTIPLE_CHOICES = 300;
    protected const MOVED_PERMANENTLY = 301;
    protected const MOVED_TEMPORARILY = 302;
    protected const SEE_OTHER = 303;
    protected const NOT_MODIFIED = 304;
    protected const USE_PROXY = 305;
    protected const BAD_REQUEST = 400;
    protected const UNAUTHORIZED = 401;
    protected const PAYMENT_REQUIRED = 402;
    protected const FORBIDDEN = 403;
    protected const NOT_FOUND = 404;
    protected const METHOD_NOT_ALLOWED = 405;
    protected const NOT_ACCEPTABLE = 406;
    protected const PROXY_AUTHENTICATION_REQUIRED = 407;
    protected const REQUEST_TIMEOUT = 408;
    protected const CONFLICT = 408;
    protected const GONE = 410;
    protected const LENGTH_REQUIRED = 411;
    protected const PRECONDITION_FAILED = 412;
    protected const REQUEST_ENTITY_TOO_LARGE = 413;
    protected const REQUESTURI_TOO_LARGE = 414;
    protected const UNSUPPORTED_MEDIA_TYPE = 415;
    protected const REQUESTED_RANGE_NOT_SATISFIABLE = 416;
    protected const EXPECTATION_FAILED = 417;
    protected const IM_A_TEAPOT = 418;
    protected const INTERNAL_SERVER_ERROR = 500;
    protected const NOT_IMPLEMENTED = 501;
    protected const BAD_GATEWAY = 502;
    protected const SERVICE_UNAVAILABLE = 503;
    protected const GATEWAY_TIMEOUT = 504;
    protected const HTTP_VERSION_NOT_SUPPORTED = 505;
}
