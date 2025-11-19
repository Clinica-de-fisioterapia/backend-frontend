import { createLogger, transports, format } from 'winston';

const logger = createLogger({
    level: 'error',
    format: format.combine(
        format.timestamp(),
        format.json()
    ),
    transports: [
        new transports.Console(),
        new transports.File({ filename: 'error.log' })
    ],
});

export const logError = (error: Error) => {
    logger.error({
        message: error.message,
        stack: error.stack,
    });
};