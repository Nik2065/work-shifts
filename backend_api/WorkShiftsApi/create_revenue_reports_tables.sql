-- Создание таблицы для отметок об оплате work_hours
CREATE TABLE IF NOT EXISTS `revenue_reports_wh` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `work_hours_id` INT NOT NULL,
  `wh_hours` INT NOT NULL,
  `wh_rate` INT NOT NULL,
  `wh_sum` INT NOT NULL,
  `wh_work_date` DATETIME NOT NULL,
  `report_number` INT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `idx_work_hours_id` (`work_hours_id`),
  INDEX `idx_wh_work_date` (`wh_work_date`),
  INDEX `idx_report_number` (`report_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Создание таблицы для отметок об оплате fin_operations
CREATE TABLE IF NOT EXISTS `revenue_reports_fin` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fin_operation_id` INT NOT NULL,
  `fo_sum` INT NOT NULL,
  `fo_is_penalty` TINYINT(1) NOT NULL,
  `fo_type_id` INT NULL,
  `report_number` INT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `idx_fin_operation_id` (`fin_operation_id`),
  INDEX `idx_report_number` (`report_number`),
  INDEX `idx_fo_type_id` (`fo_type_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Таблица номеров отчетов (история сохраненных отчетов)
CREATE TABLE IF NOT EXISTS `main_report_numbers` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `create_author` VARCHAR(255) NULL,
  `report_number` INT NOT NULL,
  `start_date` DATE NOT NULL,
  `end_date` DATE NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `idx_report_number` (`report_number`),
  INDEX `idx_created` (`created`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
